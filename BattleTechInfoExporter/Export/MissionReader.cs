using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using Contract = BattleTech.Contract;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the mission file's models from a contract the game has completed.</summary>
internal static class MissionReader
{
    // Contract.CompleteContract has filled in every value read here; GenerateSalvage, at its end, the salvage.
    internal static MissionOutcome ReadOutcome(SimGameState simGame, Contract contract)
    {
        var (employer, target) = ContractReader.ReadFactions(contract);
        return new MissionOutcome(
            ModAssembly.Version,
            ExportTrigger.MissionCompleted,
            ContractReader.ReadMissionContract(simGame, contract),
            contract.State,
            contract.IsGoodFaithEffort,
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
            // Before the choice, SalvageResults holds only the components recovered from the company's lost mechs.
            new MissionSalvage(
                contract.FinalSalvageCount,
                contract.FinalPrioritySalvageCount,
                ReadSalvageItems(simGame, contract.GetPotentialSalvage()),
                ReadSalvageItems(simGame, contract.SalvageResults),
                null));
    }

    // Contract.FinalizeSalvage moves the priority and the random salvage from the pool into SalvageResults, one
    // entry per item, so the offer can't be read again afterwards.
    internal static SalvageItems ReadSalvageReceived(SimGameState simGame, Contract contract) =>
        ReadSalvageItems(simGame, contract.SalvageResults);

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
                    mech.GetLocationLoadoutDef(location).CurrentInternalStructure))
                .Where(damaged => damaged.Structure < mech.GetChassisLocationDef(damaged.Location).InternalStructure)
                .ToList(),
            mech.Inventory
                .Where(component => component.DamageLevel != ComponentDamageLevel.Functional)
                .Select(component => new DamagedComponent(
                    ComponentReferences.ReferenceTo(component),
                    component.MountedLocation,
                    component.DamageLevel))
                .OrderByComponent(component => component.Component)
                .ToList());
    }

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
                .Select(copies => new SalvagedComponent(
                    ComponentReferences.ReferenceTo(
                        copies.First().ComponentType,
                        copies.Key,
                        copies.First().MechComponentDef),
                    copies.Where(copy => !copy.Damaged).Sum(copy => copy.Count),
                    copies.Where(copy => copy.Damaged).Sum(copy => copy.Count)))
                .OrderByComponent(component => component.Component)
                .ToList(),
            salvage
                .Where(item => item.Type == SalvageDef.SalvageType.MECH_PART)
                .GroupBy(item => item.Description.Id)
                .Select(parts => MechReader.TryReferenceToMech(simGame.DataManager, parts.Key) is { } mech
                    ? new SalvagedMechParts(mech, parts.Sum(part => part.Count))
                    : null)
                .OfType<SalvagedMechParts>()
                .OrderByDefinition(parts => parts.Mech)
                .ToList());
}
