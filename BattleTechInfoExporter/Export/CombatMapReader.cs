using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using HBS.Math;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the combat map file's model from the battle's map once it has loaded.</summary>
internal static class CombatMapReader
{
    internal static CombatMap Read(CombatGameState combat)
    {
        var buildings = BuildingReader.ReadAll(combat);
        var buildingIds = buildings.ToDictionary(
            BuildingReader.ObstructionIdOf,
            building => building.GUID,
            StringComparer.Ordinal);
        var pathingGroups = HexStepReader.ReadPathingGroups(combat);
        var stepReaders = pathingGroups.Select(pathingGroup => new HexStepReader(combat, pathingGroup[0])).ToList();
        var playableHexes = MapHexReader.ReadPlayableHexes(combat).ToList();
        var playableHexSet = new HashSet<HexPoint3>(playableHexes);

        return new CombatMap(
            MapReader.ReadMapId(combat.ActiveContract),
            ReadHexGridLayout(combat),
            pathingGroups.Select(pathingGroup => pathingGroup.Select(pathing => pathing.Description.Id).ToList())
                .ToList(),
            buildings.Select(BuildingReader.ReadBuilding).ToList(),
            playableHexes
                .Select(hex =>
                {
                    var ground = MapHexReader.ReadHex(combat, hex);
                    return new CombatMapHex(
                        ground.Q,
                        ground.R,
                        ground.Elevation,
                        ground.Terrain,
                        BuildingReader.ReadObstructionId(MapHexReader.CenterCell(combat, hex)) is { } obstructionId
                        && buildingIds.TryGetValue(obstructionId, out var buildingId)
                            ? buildingId
                            : null,
                        stepReaders.Select(stepReader => stepReader.ReadBlockedSteps(hex, playableHexSet)).ToList());
                })
                .ToList());
    }

    // HexGrid.HexAxialToCartesian.
    private static HexGridLayout ReadHexGridLayout(CombatGameState combat)
    {
        var size = combat.HexGrid.HexWidth;
        var sizeText = size.ToString(CultureInfo.InvariantCulture);
        return new HexGridLayout(
            size,
            "pointyTop",
            $"x = {sizeText} * (q + r / 2), z = {sizeText} * sqrt(3) / 2 * r: hex (0, 0) is at the map's center, "
            + "x 0, z 0",
            HexStepReader.Directions.Select(direction => new HexOffset(direction.q, direction.r)).ToList());
    }
}
