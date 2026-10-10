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
        IReadOnlyDictionary<string, CombatUnit> units,
        IReadOnlyList<BattleTech.Building> buildings)
    {
        var buildingIds = new HashSet<string>(buildings.Select(building => building.GUID));
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
            .Select(objective => ReadObjective(objective, units, buildingIds))
            .ToList();
    }

    // Shown as RegionRenderer draws them: a region whose display isn't hidden, active or as a preview.
    internal static List<ObjectiveZone> ReadZones(CombatGameState combat, IReadOnlyList<CombatObjective> objectives)
    {
        var listedObjectiveIds = new HashSet<string>(objectives.Select(objective => objective.Id));
        return combat.RegionsList
            .Where(region => region.IsRegionDisplayVisible)
            .Select(region => new ObjectiveZone(
                region.GUID,
                region.regionDefId,
                CombatUnitReader.ReadPosition(region.Position),
                region.radius,
                region.IsShowingPreviewOfRegion,
                // A region can also belong to an objective the HUD doesn't list, e.g. the evac chunk's hidden
                // objective that calls the dropship (DropshipExtractionChunkGameLogic.callDropshipObjectiveRef).
                // RegionGameLogic.AttachRegionToObjective adds an objective once per call.
                region.objectiveRefList
                    .Select(objective => objective.EncounterObjectGuid)
                    .Where(listedObjectiveIds.Contains)
                    .Distinct()
                    .ToList()))
            .ToList();
    }

    // The progress line as CombatHUDObjectiveItem shows it. The targets are the units and buildings carrying the
    // objective's tags (ObjectiveGameLogic.GetTaggedCombatants).
    private static CombatObjective ReadObjective(
        ObjectiveGameLogic objective,
        IReadOnlyDictionary<string, CombatUnit> units,
        HashSet<string> buildingIds)
    {
        var progress = objective.showProgress ? GameText.ToPlainText(objective.GetProgressText().ToString()) : null;
        var targets = objective.GetTargetUnits();
        return new CombatObjective(
            objective.GUID,
            GameText.ToPlainText(objective.title),
            objective.CurrentObjectiveStatus,
            objective.IsRequiredForPrimaryContractObjective,
            string.IsNullOrEmpty(progress) ? null : progress,
            targets
                .OfType<AbstractActor>()
                .Select(unit => unit.GUID)
                // A blip's side isn't something the HUD shows.
                .Where(id => units.TryGetValue(id, out var unit) && unit.Visibility == UnitVisibility.Full)
                .ToList(),
            targets
                .OfType<BattleTech.Building>()
                .Select(building => building.GUID)
                .Where(buildingIds.Contains)
                .ToList());
    }
}
