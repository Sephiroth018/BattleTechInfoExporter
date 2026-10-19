using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Reads the battle's buildings: the game's combatant <see cref="BattleTech.Building" /> of every obstruction with a
///     representation whose building is enabled, which stays a combatant once destroyed. Crates, trees and fences
///     aren't buildings.
/// </summary>
internal static class BuildingReader
{
    /// <summary>Every building, in the order of their ids; the combat state's lists are drawn from it.</summary>
    // A building is on the map when the obstruction's cells are enabled. A dropship's are enabled only while it is
    // landed (DropshipGameLogic.ShowDropshipBasedOnAnimationState): off the map or hovering, it occupies no cell and
    // can't be targeted. A dropship spawned at runtime for a lance or a pickup has no cells at all, which
    // ObstructionGameLogic.IsBuildingEnabled alone counts as enabled.
    internal static IReadOnlyList<BattleTech.Building> ReadAll(CombatGameState combat) =>
        combat.GetAllMiscCombatants()
            .OfType<BattleTech.Building>()
            .Where(building => ObstructionOf(combat, building) is { IsBuildingEnabled: true, occupiedCells.Count: > 0 })
            .OrderBy(building => building.GUID, StringComparer.Ordinal)
            .ToList();

    // A building can also be destroyed without damage (Building.IsDead), e.g. with the one it stands on. Damage below
    // the rounding doesn't count, so a damaged building never shows its max structure.
    internal static IReadOnlyList<DamagedBuilding> ReadDamagedBuildings(
        CombatGameState combat,
        IReadOnlyList<BattleTech.Building> buildings) =>
        buildings
            .Where(building => building.IsDead || ReadStructure(building) < ReadMaxStructure(building))
            .Select(building => new DamagedBuilding(
                building.GUID,
                ReadStructure(building),
                building.IsDead ? ReadDestroyedHexes(combat, building) : null))
            .ToList();

    // Every building is on a team from its creation (ObstructionGameLogic.ContractInitialize), the world team unless
    // the obstruction or the mission's AssignBuildingsToTeamResult puts it on one with a side, which highlights it
    // in the team's color (Building.AddToTeam). Only those are listed: urban maps have hundreds of buildings.
    internal static IReadOnlyList<BuildingSide> ReadSides(
        CombatGameState combat,
        IReadOnlyList<BattleTech.Building> buildings) =>
        buildings
            .Where(building => building.team.GUID != TeamDefinition.WorldTeamDefinitionGuid)
            .Select(building => new BuildingSide(building.GUID, CombatUnitReader.ReadSide(combat, building.team)))
            .ToList();

    internal static CombatBuilding ReadBuilding(CombatGameState combat, BattleTech.Building building) =>
        new(
            building.GUID,
            building.DisplayName,
            MapHexReader.ReadHexAt(combat, building.CurrentPosition),
            ReadMaxStructure(building));

    /// <summary>
    ///     The id of the obstruction whose building a unit standing in the cell is on, its topmost one
    ///     (<see cref="AbstractActor.CheckEnteredCellsForBuildings" />); <c>null</c> where there is none.
    /// </summary>
    internal static string? ReadObstructionId(MapTerrainDataCell cell) =>
        cell.MapEncounterLayerDataCell.GetTopmostBuilding()?.buildingGuid;

    /// <summary>The obstruction id cells name a building by: its id without the building suffix.</summary>
    internal static string ObstructionIdOf(BattleTech.Building building) =>
        ObstructionGameLogic.GetObstructionGuid(building.GUID);

    // The game updates the cells a building occupied when it falls (ObstructionGameLogic.BuildingDestroyedUpdateCells),
    // but knows no hexes of a building: a hex is the building's when its center cell is one of them. They are searched
    // within the cells' bounds, which the corners of its line of sight targets mark at the cells' centers
    // (ObstructionGameLogic.CalculateLOSTargets); a building of a solid obstruction or foundation has none, so its
    // hexes are searched on the whole map.
    private static List<MapHex> ReadDestroyedHexes(CombatGameState combat, BattleTech.Building building)
    {
        var corners = building.LOSTargetPositions;
        var occupiedCells = new HashSet<MapEncounterLayerDataCell>(ObstructionOf(combat, building).occupiedCells);
        var halfCell = MapMetaDataExporter.cellSize / 2f;
        var candidateHexes = corners is { Length: > 0 }
            ? MapHexReader.ReadPlayableHexesWithin(
                combat,
                corners.Min(corner => corner.x) - halfCell,
                corners.Max(corner => corner.x) + halfCell,
                corners.Min(corner => corner.z) - halfCell,
                corners.Max(corner => corner.z) + halfCell)
            : MapHexReader.ReadPlayableHexes(combat);
        return candidateHexes
            .Where(hex => occupiedCells.Contains(MapHexReader.CenterCell(combat, hex).MapEncounterLayerDataCell))
            .Select(hex => MapHexReader.ReadHex(combat, hex))
            .ToList();
    }

    private static float ReadStructure(BattleTech.Building building) =>
        Rounding.ToTenth(Math.Max(0f, building.CurrentStructure));

    private static float ReadMaxStructure(BattleTech.Building building) =>
        Rounding.ToTenth(building.StartingStructure);

    private static ObstructionGameLogic ObstructionOf(CombatGameState combat, BattleTech.Building building) =>
        ObstructionGameLogic.GetObstructionFromBuilding(building, combat.ItemRegistry)
        ?? throw new InvalidOperationException($"Building {building.GUID} has no obstruction");
}
