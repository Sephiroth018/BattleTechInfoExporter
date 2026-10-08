using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Reads the battle's buildings: the game's combatant <see cref="BattleTech.Building" /> of every obstruction with a
///     representation, which stays a combatant once destroyed. Crates, trees and fences aren't buildings.
/// </summary>
internal static class BuildingReader
{
    /// <summary>Every building, in the order of their ids.</summary>
    internal static IReadOnlyList<BattleTech.Building> ReadAll(CombatGameState combat) =>
        combat.GetAllMiscCombatants()
            .OfType<BattleTech.Building>()
            .OrderBy(building => building.GUID, StringComparer.Ordinal)
            .ToList();

    internal static CombatBuilding ReadBuilding(BattleTech.Building building) =>
        new(
            building.GUID,
            building.DisplayName,
            CombatUnitReader.ReadPosition(building.CurrentPosition),
            building.StartingStructure);

    /// <summary>
    ///     The id of the obstruction whose building a unit standing in the cell is on, its topmost one
    ///     (AbstractActor.CheckEnteredCellsForBuildings); <c>null</c> where there is none.
    /// </summary>
    internal static string? ReadObstructionId(MapTerrainDataCell cell) =>
        cell.MapEncounterLayerDataCell.GetTopmostBuilding()?.buildingGuid;

    /// <summary>The obstruction id cells name a building by: its id without the building suffix.</summary>
    internal static string ObstructionIdOf(BattleTech.Building building) =>
        ObstructionGameLogic.GetObstructionGuid(building.GUID);

    internal static IReadOnlyList<DamagedBuilding> ReadDamagedBuildings(CombatGameState combat)
    {
        return ReadAll(combat)
            .Where(building => building.CurrentStructure < building.StartingStructure)
            .Select(building => new DamagedBuilding(
                building.GUID,
                Math.Max(0f, building.CurrentStructure),
                building.IsDead ? ReadDestroyedHexes(combat, building) : null))
            .ToList();
    }

    // The game updates the cells a building occupied when it falls (ObstructionGameLogic.BuildingDestroyedUpdateCells),
    // but knows no hexes of a building: a hex is the building's when its center cell is one of them. They are searched
    // within the cells' bounds, which the corners of its line of sight targets mark at the cells' centers
    // (ObstructionGameLogic.CalculateLOSTargets).
    private static List<MapHex> ReadDestroyedHexes(CombatGameState combat, BattleTech.Building building)
    {
        var corners = building.LOSTargetPositions;
        if (corners is not { Length: > 0 })
        {
            // Only obstructions without a representation, which have no building, lack them.
            ModLog.Logger.LogWarning($"Left out the hexes of destroyed building {building.GUID}: it has no bounds");
            return [];
        }

        var occupiedCells = new HashSet<MapEncounterLayerDataCell>(ObstructionOf(combat, building).occupiedCells);
        var halfCell = MapMetaDataExporter.cellSize / 2f;
        return MapHexReader.ReadPlayableHexesWithin(
                combat,
                corners.Min(corner => corner.x) - halfCell,
                corners.Max(corner => corner.x) + halfCell,
                corners.Min(corner => corner.z) - halfCell,
                corners.Max(corner => corner.z) + halfCell)
            .Select(hex => (Hex: hex, Cell: MapHexReader.CenterCell(combat, hex)))
            .Where(hex => occupiedCells.Contains(hex.Cell.MapEncounterLayerDataCell))
            .Select(hex => new MapHex(
                hex.Hex.q,
                hex.Hex.r,
                MapHexReader.ReadElevation(hex.Cell),
                MapHexReader.ReadTerrainId(combat, hex.Cell)))
            .ToList();
    }

    private static ObstructionGameLogic ObstructionOf(CombatGameState combat, BattleTech.Building building) =>
        ObstructionGameLogic.GetObstructionFromBuilding(building, combat.ItemRegistry)
        ?? throw new InvalidOperationException($"Building {building.GUID} has no obstruction");
}
