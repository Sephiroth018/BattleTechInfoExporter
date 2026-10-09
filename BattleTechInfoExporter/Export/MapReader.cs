using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;
using HBS.Util;
using Contract = BattleTech.Contract;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds the catalog's map definitions from the maps the contract generator selects for each star system
///     definition, the metadata database's map rows and each map's terrain data file, which the game reads only when
///     the map is played (MapMetaData.LoadMapMetaData).
/// </summary>
internal static class MapReader
{
    private const string NoTerrainKey = "none";

    // The terrain data resource of a map is named after the map's scene (MapMetaDataExporter.GetTerrainDataPath).
    private const string TerrainDataIdSuffix = "_TerrainData";

    // The masks of the flags no map names its own mask for (MapMetaData.Load).
    private const string ImpassableMaskId = "DesignMaskImpassable";
    private const string DestroyedBuildingMaskId = "DesignMaskDestroyedBuilding";

    /// <summary>
    ///     Every map the contracts of some star system definition can be fought on, keyed by map id; the star systems
    ///     are those of <see cref="StarSystemDefinitionReader" />.
    /// </summary>
    internal static SortedDictionary<string, MapDefinition> ReadMapDefinitions(
        DataManager dataManager,
        IReadOnlyDictionary<string, TerrainDefinition> terrains)
    {
        var starSystemsPerMap = dataManager.SystemDefs
            .SelectMany(definition =>
            {
                var starSystem = DefinitionReferences.ReferenceTo(definition.Value.Description);
                return ReadPlayableMaps(definition.Value).Select(map => (Map: map, StarSystem: starSystem));
            })
            .GroupBy(playable => playable.Map.MapID, StringComparer.Ordinal);

        var maps = new SortedDictionary<string, MapDefinition>(StringComparer.Ordinal);
        foreach (var starSystems in starSystemsPerMap)
        {
            var map = starSystems.First().Map;
            if (ReadTerrainCoverage(dataManager, map, terrains) is not { } terrainCoverage)
            {
                continue;
            }

            var tags = MetadataDatabase.Instance.GetTagSetForTagSetEntry(map.TagSetID)
                .OrderBy(tag => tag, StringComparer.Ordinal)
                .ToList();
            maps.Add(
                map.MapID,
                new MapDefinition(
                    map.FriendlyName,
                    DefinitionReferences.ReferenceTo(dataManager, (Biome.BIOMESKIN)map.BiomeSkinID),
                    tags,
                    starSystems.Select(playable => playable.StarSystem)
                        .OrderBy(starSystem => starSystem)
                        .ToList(),
                    map.Weight,
                    terrainCoverage));
        }

        return maps;
    }

    /// <summary>The id the catalog keys the contract's map by, its MapID; the contract knows a map only by its path.</summary>
    internal static string? ReadMapId(Contract contract) =>
        MetadataDatabase.Instance.GetMapByPath(contract.mapPath)?.MapID;

    // The maps the contract generator draws a star system's contracts from
    // (SimGameState.GetSinglePlayerProceduralPlayableMaps): the released maps with a procedural encounter whose
    // biome the system supports and whose tags its required and excluded map tags allow, DLC ownership included.
    private static IEnumerable<Map_MDD> ReadPlayableMaps(StarSystemDef starSystem) =>
        MetadataDatabase.Instance
            .GetReleasedMapsAndEncountersBySinglePlayerProceduralContractTypeAndTags(
                starSystem.MapRequiredTags,
                starSystem.MapExcludedTags,
                starSystem.SupportedBiomes,
                true)
            .Select(mapAndEncounters => mapAndEncounters.Map);

    private static SortedDictionary<string, double>? ReadTerrainCoverage(
        DataManager dataManager,
        Map_MDD map,
        IReadOnlyDictionary<string, TerrainDefinition> terrains)
    {
        var terrainDataId = map.MapName + TerrainDataIdSuffix;
        var entry = dataManager.ResourceLocator.EntryByID(terrainDataId, BattleTechResourceType.TerrainData);
        if (entry is null || !File.Exists(entry.FilePath))
        {
            ModLog.Logger.LogWarning(
                $"Left out map {map.MapID}: its terrain data {terrainDataId} isn't in the manifest or on disk");
            return null;
        }

        var cellsPerTerrain = CountCellsPerTerrain(File.ReadAllBytes(entry.FilePath));
        var playableCells = cellsPerTerrain.Values.Sum();
        var terrainCoverage = new SortedDictionary<string, double>(StringComparer.Ordinal);
        foreach (var cells in cellsPerTerrain)
        {
            if (cells.Key != NoTerrainKey && !terrains.ContainsKey(cells.Key))
            {
                ModLog.Logger.LogWarning(
                    $"Map {map.MapID} has cells of terrain {cells.Key}, which isn't in the catalog");
            }

            terrainCoverage.Add(cells.Key, (double)cells.Value / playableCells);
        }

        return terrainCoverage;
    }

    // Reads the file as MapMetaData.Load does, but without building a cell object per cell (a quarter million a
    // map) or setting the game's static water-is-ice state.
    private static Dictionary<string, int> CountCellsPerTerrain(byte[] terrainData)
    {
        using var stream = new SerializationStream(terrainData);
        stream.GetString(); // The map name.
        stream.GetString(); // The biome's mask, which is no terrain.
        var maskIds = new Dictionary<TerrainMaskFlags, string>
        {
            [TerrainMaskFlags.Water] = stream.GetString(),
            [TerrainMaskFlags.DeepWater] = stream.GetString(),
            [TerrainMaskFlags.Forest] = stream.GetString(),
            [TerrainMaskFlags.Rough] = stream.GetString(),
            [TerrainMaskFlags.Custom] = stream.GetString(),
            [TerrainMaskFlags.Road] = stream.GetString(),
            [TerrainMaskFlags.Impassable] = ImpassableMaskId,
            [TerrainMaskFlags.DestroyedBuilding] = DestroyedBuildingMaskId,
            [TerrainMaskFlags.None] = NoTerrainKey
        };
        stream.GetInt(); // The cell size.
        var cellCount = stream.GetInt() * stream.GetInt();

        // Indexed by the flag's value, which is one of a few bits.
        var cellsPerFlag = new int[(int)TerrainMaskFlags.MapBoundary + 1];
        var cell = new MapTerrainDataCell();
        for (var cellIndex = 0; cellIndex < cellCount; cellIndex++)
        {
            cell.Load(stream);
            // A cell's terrain is its highest-priority flag, as in combat; the cells on the map's boundary lie
            // outside the playable area.
            var terrainFlag = MapMetaData.GetPriorityTerrainMaskFlags(cell.terrainMask);
            if (terrainFlag != TerrainMaskFlags.MapBoundary)
            {
                cellsPerFlag[(int)terrainFlag]++;
            }
        }

        var cellsPerTerrain = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var maskId in maskIds.Where(maskId => cellsPerFlag[(int)maskId.Key] > 0))
        {
            cellsPerTerrain.TryGetValue(maskId.Value, out var cells);
            cellsPerTerrain[maskId.Value] = cells + cellsPerFlag[(int)maskId.Key];
        }

        return cellsPerTerrain;
    }
}
