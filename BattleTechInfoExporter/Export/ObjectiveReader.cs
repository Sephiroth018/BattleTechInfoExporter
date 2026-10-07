using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Framework;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the combat state's objectives and zones as the HUD shows them.</summary>
internal static class ObjectiveReader
{
    // Mirrors CombatHUDObjectivesList.InitObjectives: by priority, highest first, without the objectives of contract
    // objectives that are another player's. An objective stays listed once finished, which the HUD fades out.
    internal static List<CombatObjective> ReadObjectives(
        CombatGameState combat,
        IReadOnlyDictionary<string, CombatUnit> units)
    {
        var otherPlayersObjectiveIds = new HashSet<string>(
            combat.ItemRegistry
                .GetObjectsOfType<ContractObjectiveGameLogic>(TaggedObjectType.ContractObjective)
                .Where(contractObjective => !combat.IsLocalPlayerTeam(contractObjective.forPlayer))
                .SelectMany(contractObjective => contractObjective.objectiveRefList)
                .Select(objective => objective.EncounterObjectGuid));
        return combat.ItemRegistry
            .GetObjectsOfType<ObjectiveGameLogic>(TaggedObjectType.Objective)
            .Where(objective => objective.DisplayInObjectiveList
                                && !otherPlayersObjectiveIds.Contains(objective.encounterObjectGuid))
            .OrderByDescending(objective => objective.priority)
            .Select(objective => ReadObjective(objective, units))
            .ToList();
    }

    // Shown as RegionRenderer draws them: a region whose display isn't hidden, active or as a preview.
    internal static List<ObjectiveZone> ReadZones(CombatGameState combat) =>
        combat.RegionsList
            .Where(region => region.IsRegionDisplayVisible)
            .Select(region => new ObjectiveZone(
                region.GUID,
                region.regionDefId,
                CombatUnitReader.ReadPosition(region.Position),
                region.radius,
                region.IsShowingPreviewOfRegion,
                // RegionGameLogic.AttachRegionToObjective adds an objective once per call.
                region.objectiveRefList.Select(objective => objective.EncounterObjectGuid).Distinct().ToList()))
            .ToList();

    // The progress line as CombatHUDObjectiveItem shows it.
    private static CombatObjective ReadObjective(
        ObjectiveGameLogic objective,
        IReadOnlyDictionary<string, CombatUnit> units)
    {
        var progress = objective.showProgress ? GameText.ToPlainText(objective.GetProgressText().ToString()) : null;
        return new CombatObjective(
            objective.GUID,
            GameText.ToPlainText(objective.title),
            objective.CurrentObjectiveStatus,
            objective.IsRequiredForPrimaryContractObjective,
            string.IsNullOrEmpty(progress) ? null : progress,
            objective.GetTargetUnits()
                .OfType<AbstractActor>()
                .Select(unit => unit.GUID)
                // A blip's side isn't something the HUD shows.
                .Where(id => units.TryGetValue(id, out var unit) && unit.Visibility == UnitVisibility.Full)
                .ToList());
    }
}
