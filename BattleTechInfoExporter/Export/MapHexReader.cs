using System;
using System.Collections.Generic;
using BattleTech;
using HBS.Math;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Reads the hexes of the game's movement grid (<see cref="HexGrid" />) inside the battle's playable area, each as
///     a unit standing on it gets it: from the single cell at its center, without aggregating over the hex.
/// </summary>
internal static class MapHexReader
{
    /// <summary>Every playable hex, row by row (by <c>r</c>, then <c>q</c>).</summary>
    internal static IEnumerable<HexPoint3> ReadPlayableHexes(CombatGameState combat) =>
        ReadPlayableHexesWithin(
            combat,
            -MapMetaData.HALF_WIDTH,
            MapMetaData.HALF_WIDTH,
            -MapMetaData.HALF_WIDTH,
            MapMetaData.HALF_WIDTH);

    /// <summary>The playable hexes centered within the area in world coordinates, row by row.</summary>
    internal static IEnumerable<HexPoint3> ReadPlayableHexesWithin(
        CombatGameState combat,
        float minX,
        float maxX,
        float minZ,
        float maxZ)
    {
        var hexSize = combat.HexGrid.HexWidth;
        var rowDistance = hexSize * HexGrid.SQRT_3_OVER_2;
        // Hex centers lie at x = hexSize · (q + r / 2), z = hexSize · √3/2 · r (HexGrid.HexAxialToCartesian).
        for (var r = (int)Math.Ceiling(minZ / rowDistance); r <= (int)Math.Floor(maxZ / rowDistance); r++)
        {
            var maxQ = (int)Math.Floor(maxX / hexSize - r / 2f);
            for (var q = (int)Math.Ceiling(minX / hexSize - r / 2f); q <= maxQ; q++)
            {
                var hex = new HexPoint3(q, r);
                if (IsPlayable(combat, hex))
                {
                    yield return hex;
                }
            }
        }
    }

    internal static MapTerrainDataCell CenterCell(CombatGameState combat, HexPoint3 hex) =>
        combat.MapMetaData.GetCellAt(combat.HexGrid.HexPoint3ToCartesianWorld(hex));

    /// <summary>
    ///     The height a unit standing on the hex is at (PathNodeGrid.GetPathNode): a building's roof where one stands,
    ///     rounded to 0.1 m.
    /// </summary>
    internal static double ReadElevation(MapTerrainDataCell cell) => Math.Round(cell.cachedHeight, 1);

    /// <summary>
    ///     The terrain a unit standing on the hex is in (AbstractActor.OnPositionUpdate), keyed as in the catalog's
    ///     terrains; <c>null</c> on open ground.
    /// </summary>
    internal static string? ReadTerrainId(CombatGameState combat, MapTerrainDataCell cell) =>
        combat.MapMetaData.GetPriorityDesignMask(cell)?.Id;

    // A unit can stand on a hex inside the contract's encounter bounds (PathNode.IsLegalWorldPathLocationForActor).
    // The bounds of a contract without any are the whole map, so the cells the game doesn't read
    // (MapMetaData.IsWithinBounds) and the band on the map's boundary are left out too.
    private static bool IsPlayable(CombatGameState combat, HexPoint3 hex)
    {
        var center = combat.HexGrid.HexPoint3ToCartesianWorld(hex);
        return combat.MapMetaData.IsWithinBounds(center)
               && !SplatMapInfo.IsMapBoundary(combat.MapMetaData.GetCellAt(center).terrainMask)
               && combat.EncounterLayerData.IsInEncounterBounds(center);
    }
}
