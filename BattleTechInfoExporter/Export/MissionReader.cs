using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using Contract = BattleTech.Contract;
using Mech = BattleTech.Mech;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the mission file's models from a contract the game has completed.</summary>
internal static class MissionReader
{
    // Contract.CompleteContract has filled in every value read here, and nothing changes until the contract is
    // resolved, except the salvage: FinalizeSalvage moves the chosen items into SalvageResults, so the offer comes
    // from the caller.
    internal static MissionOutcome ReadOutcome(
        SimGameState simGame,
        Contract contract,
        SalvageOffer offer,
        IReadOnlyList<SalvageDef>? received)
    {
        var (employer, target) = ContractReader.ReadFactions(contract);
        return new MissionOutcome(
            ContractReader.ReadMissionContract(simGame, contract),
            contract.State,
            contract.IsGoodFaithEffort,
            contract.TotalCombatRounds,
            contract.MissionObjectiveResultList
                .Select(objective => new ObjectiveResult(
                    GameText.ToPlainText(objective.title),
                    objective.isPrimary,
                    objective.status))
                .ToList(),
            contract.MoneyResults,
            // SimGameState.GetFinalReputationChange gives a faction that doesn't gain reputation no change.
            new ReputationChange(
                employer.DoesGainReputation ? contract.EmployerReputationResults : null,
                target.DoesGainReputation ? contract.TargetReputationResults : null),
            contract.MercenaryReviewboardReputationResults,
            contract.ExperienceEarned,
            contract.PlayerUnitResults
                .Select(unit => ReadLanceUnit(simGame, contract, unit))
                .ToList(),
            new MissionSalvage(
                contract.FinalSalvageCount,
                contract.FinalPrioritySalvageCount,
                ReadSalvageItems(simGame, offer.Pool),
                ReadSalvageItems(simGame, offer.Automatic),
                received is null ? null : ReadSalvageItems(simGame, received)));
    }

    // The unit's mech and pilot are copies taken from combat (Mech.ToMechDef keeps the mech bay's GUID); the pilot's
    // kills count this mission only, as Pilot.InitStats resets them when combat starts.
    private static LanceUnitOutcome ReadLanceUnit(
        SimGameState simGame,
        Contract contract,
        UnitResult unit)
    {
        var pilot = unit.pilot;
        var mech = unit.mech;
        // Contract.FinalizeKilledMechWarriors has rolled who of the incapacitated dies.
        var isKilled = contract.KilledPilots.Contains(pilot);
        return new LanceUnitOutcome(
            PilotReader.ReferenceTo(pilot),
            pilot.Injuries,
            pilot.Health,
            pilot.IsIncapacitated,
            isKilled,
            isKilled ? null : PilotReader.ReadReadyOnDayAfterMission(simGame, pilot),
            pilot.MechsKilled,
            pilot.OthersKilled,
            MechReader.ReferenceToBayMech(mech),
            unit.mechLost,
            unit.mechLost ? null : MechRepair.Estimate(simGame, RestoreAfterCombat(simGame, mech)),
            MechReader.Locations
                .Select(location => new DamagedLocation(
                    location,
                    MechReader.ReadArmor(mech, location),
                    MechReader.ReadRearArmor(mech, location),
                    mech.GetLocationLoadoutDef(location).CurrentInternalStructure))
                .Where(damaged => damaged.Armor.Current < damaged.Armor.Assigned
                                  || damaged.RearArmor?.Current < damaged.RearArmor?.Assigned
                                  || mech.IsLocationDamaged(damaged.Location))
                .ToList(),
            mech.Inventory
                .Where(component => component.DamageLevel != ComponentDamageLevel.Functional)
                .Select(component => new DamagedComponent(
                    ComponentReferences.ReferenceTo(component),
                    component.MountedLocation,
                    component.DamageLevel))
                .OrderBy(component => component.Component)
                .ToList(),
            ReadAmmunitionUse(FindCombatMech(contract, mech)));
    }

    // The unit's mech as it fought, which alone knows its ammo: Mech.ToMechDef copies no ammo but keeps the GUID.
    // The combat outlives the after-action report (MissionResults.ConfirmResults clears it after FinalizeSalvage),
    // so both writes of the outcome find it.
    private static Mech FindCombatMech(Contract contract, MechDef mech) =>
        contract.BattleTechGame.Combat.AllMechs.Single(combatMech => combatMech.MechDef.GUID == mech.GUID);

    private static List<AmmunitionUse> ReadAmmunitionUse(Mech combatMech) =>
        combatMech.allComponents
            .Select(component => ReadShotsFired(component) is { } shotsFired
                ? new AmmunitionUse(
                    ComponentReferences.ReferenceTo(component.mechComponentRef),
                    component.mechComponentRef.MountedLocation,
                    shotsFired)
                : null)
            .OfType<AmmunitionUse>()
            .ToList();

    // AmmunitionBox.CurrentAmmo reports 0 for a destroyed box; its statistic keeps the rounds left when it was
    // destroyed, so the shots fired are what the capacity lost. A weapon that carries its own ammo
    // (AmmoCategoryValue.UsesInternalAmmo) starts a mission with WeaponDef.StartingAmmoCapacity. Null for a
    // component without ammo.
    private static int? ReadShotsFired(MechComponent component) =>
        component switch
        {
            AmmunitionBox box => box.AmmoCapacity - box.StatCollection.GetValue<int>("CurrentAmmo"),
            Weapon { AmmoCategoryValue.UsesInternalAmmo: true } weapon =>
                weapon.weaponDef.StartingAmmoCapacity - weapon.InternalAmmo,
            _ => null
        };

    // The mech as the mech bay gets it back, with its non-functional components working again
    // (SimGameState.ResolveCompleteContract); restored on a copy, as the contract's results stay as they are.
    private static MechDef RestoreAfterCombat(SimGameState simGame, MechDef mech)
    {
        var restored = new MechDef(mech);
        simGame.RestoreMechPostCombat(restored);
        return restored;
    }

    // A component's salvage id is the component's, a mech part's the mech's it assembles into
    // (SimGameState.ResolveCompleteContract). Chassis salvage is never generated (Contract.GenerateSalvage).
    private static SalvageItems ReadSalvageItems(SimGameState simGame, IReadOnlyList<SalvageDef> salvage) =>
        new(
            salvage
                .Where(item => item.Type == SalvageDef.SalvageType.COMPONENT)
                .GroupBy(item => item.Description.Id)
                .Select(copies => new StoredComponent(
                    ComponentReferences.ReferenceTo(
                        copies.First().ComponentType,
                        copies.Key,
                        copies.First().MechComponentDef),
                    copies.Where(copy => !copy.Damaged).Sum(copy => copy.Count),
                    copies.Where(copy => copy.Damaged).Sum(copy => copy.Count)))
                .OrderBy(component => component.Component)
                .ToList(),
            ReferencedEntries.Read(
                    salvage
                        .Where(item => item.Type == SalvageDef.SalvageType.MECH_PART)
                        .GroupBy(item => item.Description.Id),
                    parts => MechReader.TryReferenceToMech(simGame.DataManager, parts.Key),
                    (mech, parts) => new StoredMechParts(mech, parts.Sum(part => part.Count)))
                .OrderBy(parts => parts.Mech)
                .ToList());
}
