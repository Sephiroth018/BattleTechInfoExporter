using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;
using Starmap = BattleTechInfoExporter.Models.Starmap;
using StarSystem = BattleTech.StarSystem;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the star systems file's model from the game's starmap.</summary>
internal static class StarSystemReader
{
    internal static Starmap Read(SimGameState simGame)
    {
        var visibleTags = ReadVisibleTagsByName();
        // The few biomes are resolved once instead of per star system: each lookup builds the game's biome table
        // anew (DataManagerExtensions.GetBaseDescriptionDef).
        var biomes = simGame.StarSystems
            .SelectMany(system => system.Def.SupportedBiomes)
            .Distinct()
            .ToDictionary(biome => biome, biome => DefinitionReferences.ReferenceTo(simGame.DataManager, biome));
        var starSystems = new SortedDictionary<string, Models.StarSystem>(StringComparer.Ordinal);
        foreach (var system in simGame.StarSystems)
        {
            starSystems.Add(system.Def.Description.Id, ReadStarSystem(simGame, system, visibleTags, biomes));
        }

        return new Starmap(starSystems);
    }

    // A star system's tags are its active definition's, also after a swap (StarSystem.SetNewStarSystemDef).
    // StarSystemDef.SupportedBiomes limits the maps of the star system's contracts
    // (SimGameState.GetSinglePlayerProceduralPlayableMaps).
    private static Models.StarSystem ReadStarSystem(
        SimGameState simGame,
        StarSystem system,
        IReadOnlyDictionary<string, DefinitionReference> visibleTags,
        Dictionary<Biome.BIOMESKIN, DefinitionReference> biomes)
    {
        var definition = system.Def;
        var canTravelTo = simGame.Starmap.CanTravelToNode(system.ID);
        // The starmap offers no trip to the current system (SGNavigationScreen.OnSystemRouted).
        var route = system.ID == simGame.CurSystem.ID ? new Route(0, 0)
            : canTravelTo ? RouteReader.ReadRoute(simGame, system)
            : null;
        return new Models.StarSystem(
            definition.Description.Id,
            definition.Description.Name,
            DefinitionReferences.ReferenceTo(definition.OwnerValue),
            ReadVisibleTags(definition, visibleTags),
            definition.SupportedBiomes.Select(biome => biomes[biome]).ToList(),
            // As the starmap's system panel shows it (SGSystemViewPopulator).
            simGame.GetNormalizedDifficulty(definition),
            canTravelTo,
            route);
    }

    // Mirrors the starmap's system panel (SGSystemViewPopulator, HBSTagView), which shows only the tags the
    // metadata database marks as player-visible, by their friendly name (TagDataStructFetcher.GetItem).
    private static Dictionary<string, DefinitionReference> ReadVisibleTagsByName() =>
        // One query instead of one per tag (GetTagIfExists); the Name column compares binary, as Ordinal does.
        MetadataDatabase.Instance.GetAllTags()
            .Where(tag => tag.PlayerVisible)
            .ToDictionary(
                tag => tag.Name,
                tag => new DefinitionReference(tag.Name, tag.FriendlyName),
                StringComparer.Ordinal);

    private static List<DefinitionReference> ReadVisibleTags(
        StarSystemDef definition,
        IReadOnlyDictionary<string, DefinitionReference> visibleTags) =>
        definition.Tags
            .Where(visibleTags.ContainsKey)
            .Select(tag => visibleTags[tag])
            .ToList();
}
