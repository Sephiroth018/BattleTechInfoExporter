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
    private const string OpenGroundCharacter = ".";

    // One per terrain on the map, in the order of the terrain ids; a map has about ten.
    private const string TerrainCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // A blocked steps mask (0 to 63) is written as its base64 digit, none of which JSON escapes.
    private const string MaskCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    internal static CombatMap Read(CombatGameState combat)
    {
        var buildings = BuildingReader.ReadAll(combat);
        var buildingIndices = buildings
            .Select((building, index) => (ObstructionId: BuildingReader.ObstructionIdOf(building), Index: index))
            .ToDictionary(building => building.ObstructionId, building => building.Index, StringComparer.Ordinal);
        var pathingGroups = HexStepReader.ReadPathingGroups(combat);
        var stepReaders = pathingGroups.Select(pathingGroup => new HexStepReader(combat, pathingGroup[0])).ToList();
        var playableHexes = MapHexReader.ReadPlayableHexes(combat).ToList();
        var playableHexSet = new HashSet<HexPoint3>(playableHexes);
        var hexes = playableHexes
            .Select(hex => new RowHex(
                MapHexReader.ReadHex(combat, hex),
                BuildingReader.ReadObstructionId(MapHexReader.CenterCell(combat, hex)) is { } obstructionId
                && buildingIndices.TryGetValue(obstructionId, out var index)
                    ? index
                    : null,
                stepReaders.Select(stepReader => stepReader.ReadBlockedSteps(hex, playableHexSet)).ToList()))
            .ToList();
        var terrainCharacters = AssignTerrainCharacters(hexes);

        return new CombatMap(
            MapReader.ReadMapId(combat.ActiveContract),
            ReadHexGridLayout(combat),
            new SortedDictionary<string, string?>(
                terrainCharacters.ToDictionary(terrain => terrain.Value, string? (terrain) => terrain.Key),
                StringComparer.Ordinal) { [OpenGroundCharacter] = null },
            pathingGroups.Select(pathingGroup => pathingGroup.Select(pathing => pathing.Description.Id).ToList())
                .ToList(),
            buildings.Select(BuildingReader.ReadBuilding).ToList(),
            ReadRows(hexes, terrainCharacters));
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
            HexStepReader.Directions.Select(direction => new HexOffset(direction.q, direction.r)).ToList(),
            "each character is a base64 digit (A = 0, B = 1, ... / = 63) whose bit i is set when the step to the "
            + "neighbor at directions[i] is blocked; a step to a hex not listed is blocked too");
    }

    private static Dictionary<string, string> AssignTerrainCharacters(IEnumerable<RowHex> hexes)
    {
        var terrainIds = hexes
            .Select(hex => hex.Hex.Terrain)
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(terrainId => terrainId, StringComparer.Ordinal)
            .ToList();
        if (terrainIds.Count > TerrainCharacters.Length)
        {
            throw new InvalidOperationException(
                $"The map has {terrainIds.Count} terrains, more than the {TerrainCharacters.Length} characters for them");
        }

        return terrainIds
            .Select((terrainId, index) => (TerrainId: terrainId, Character: TerrainCharacters[index].ToString()))
            .ToDictionary(terrain => terrain.TerrainId, terrain => terrain.Character, StringComparer.Ordinal);
    }

    // The hexes come row by row (MapHexReader.ReadPlayableHexes); a row continues while they are neighbors.
    private static List<HexRow> ReadRows(List<RowHex> hexes, Dictionary<string, string> terrainCharacters)
    {
        var rows = new List<HexRow>();
        var start = 0;
        for (var end = 1; end <= hexes.Count; end++)
        {
            if (end < hexes.Count
                && hexes[end].Hex.R == hexes[end - 1].Hex.R
                && hexes[end].Hex.Q == hexes[end - 1].Hex.Q + 1)
            {
                continue;
            }

            var rowHexes = hexes.GetRange(start, end - start);
            rows.Add(
                new HexRow(
                    rowHexes[0].Hex.R,
                    rowHexes[0].Hex.Q,
                    string.Concat(rowHexes.Select(hex =>
                        hex.Hex.Terrain is { } terrainId ? terrainCharacters[terrainId] : OpenGroundCharacter)),
                    rowHexes.Select(hex => hex.Hex.Elevation).ToList(),
                    rowHexes.Select(hex => hex.BuildingIndex).ToList(),
                    Enumerable.Range(0, rowHexes[0].BlockedSteps.Count)
                        .Select(group => string.Concat(rowHexes.Select(hex =>
                            MaskCharacters[hex.BlockedSteps[group]])))
                        .ToList()));
            start = end;
        }

        return rows;
    }

    // A hex as it goes into its row; BlockedSteps holds a mask per pathing group (HexStepReader.ReadBlockedSteps).
    private sealed record RowHex(MapHex Hex, int? BuildingIndex, IReadOnlyList<int> BlockedSteps);
}
