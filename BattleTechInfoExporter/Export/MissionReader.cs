using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using Contract = BattleTech.Contract;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the mission files' models from a contract the game has completed.</summary>
internal static class MissionReader
{
    // Contract.CompleteContract has filled in every value read here; GenerateSalvage, at its end, the salvage.
    internal static MissionOutcome ReadOutcome(SimGameState simGame, Contract contract, ExportTrigger trigger)
    {
        var componentReferences = new ComponentReferences(simGame.DataManager);
        var (employer, target) = ContractReader.ReadFactions(contract);
        return new MissionOutcome(
            ModAssembly.Version,
            trigger,
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
                .Select(unit => ReadLanceUnit(simGame, componentReferences, contract, unit))
                .ToList(),
            // Before the choice, SalvageResults holds only the components recovered from the company's lost mechs.
            new SalvageOffer(
                contract.FinalSalvageCount,
                contract.FinalPrioritySalvageCount,
                ReadSalvageItems(simGame, componentReferences, contract.GetPotentialSalvage()),
                ReadSalvageItems(simGame, componentReferences, contract.SalvageResults)),
            // After every section that references components.
            componentReferences.Definitions);
    }

    // Contract.FinalizeSalvage adds the priority and the random salvage to SalvageResults, one entry per item.
    internal static SalvageReceived ReadSalvageReceived(SimGameState simGame, Contract contract, ExportTrigger trigger)
    {
        var componentReferences = new ComponentReferences(simGame.DataManager);
        return new SalvageReceived(
            ModAssembly.Version,
            trigger,
            ContractReader.ReadMissionContract(simGame, contract),
            ReadSalvageItems(simGame, componentReferences, contract.SalvageResults),
            componentReferences.Definitions);
    }

    // The unit's mech and pilot are copies taken from combat (Mech.ToMechDef keeps the mech bay's GUID); the pilot's
    // kills count this mission only, as Pilot.InitStats resets them when combat starts.
    private static LanceUnitOutcome ReadLanceUnit(
        SimGameState simGame,
        ComponentReferences componentReferences,
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
                .Select(location => new DamagedLocation(location, MechReader.ReadStructure(mech, location)))
                .Where(location => location.Structure.Current < location.Structure.Max)
                .ToList(),
            mech.Inventory
                .Where(component => component.DamageLevel != ComponentDamageLevel.Functional)
                .Select(component => new DamagedComponent(
                    componentReferences.ReferenceTo(component),
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
    private static SalvageItems ReadSalvageItems(
        SimGameState simGame,
        ComponentReferences componentReferences,
        IReadOnlyList<SalvageDef> salvage) =>
        new(
            salvage
                .Where(item => item.Type == SalvageDef.SalvageType.COMPONENT)
                .GroupBy(item => item.Description.Id)
                .Select(copies => new SalvagedComponent(
                    componentReferences.ReferenceTo(
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
