using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using Starmap = BattleTechInfoExporter.Models.Starmap;
using StarSystem = BattleTech.StarSystem;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the star systems file's model from the game's starmap.</summary>
internal static class StarSystemReader
{
    internal static Starmap Read(SimGameState simGame)
    {
        // The few biomes are resolved once instead of per star system: each lookup builds the game's biome table
        // anew (DataManagerExtensions.GetBaseDescriptionDef).
        var biomes = simGame.StarSystems
            .SelectMany(system => system.Def.SupportedBiomes)
            .Distinct()
            .ToDictionary(biome => biome, biome => DefinitionReferences.ReferenceTo(simGame.DataManager, biome));
        // The map query depends only on its inputs, which many star systems share, so it runs once per distinct input.
        var playableMaps = new Dictionary<string, List<DefinitionReference>>(StringComparer.Ordinal);
        var starSystems = new SortedDictionary<string, Models.StarSystem>(StringComparer.Ordinal);
        foreach (var system in simGame.StarSystems)
        {
            starSystems.Add(
                system.Def.Description.Id,
                ReadStarSystem(simGame, system, biomes, playableMaps));
        }

        return new Starmap(starSystems);
    }

    // A star system's tags are its active definition's, also after a swap (StarSystem.SetNewStarSystemDef).
    // StarSystemDef.SupportedBiomes limits the maps of the star system's contracts
    // (SimGameState.GetSinglePlayerProceduralPlayableMaps).
    private static Models.StarSystem ReadStarSystem(
        SimGameState simGame,
        StarSystem system,
        Dictionary<Biome.BIOMESKIN, DefinitionReference> biomes,
        Dictionary<string, List<DefinitionReference>> playableMaps)
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
            system.Tags.OrderBy(tag => tag, StringComparer.Ordinal).ToList(),
            definition.SupportedBiomes.Select(biome => biomes[biome]).OrderBy(biome => biome).ToList(),
            ReadPlayableMaps(definition, playableMaps),
            ReadStoreCollections(definition),
            // As the starmap's system panel shows it (SGSystemViewPopulator).
            simGame.GetNormalizedDifficulty(definition),
            canTravelTo,
            route);
    }

    // The stores draw from the active definition's collections (StarSystem.SetNewStarSystemDef resets them on a swap).
    // A definition without a store has null instead of an empty list (Shop.Initialize checks both).
    private static ByStore<IReadOnlyList<string>> ReadStoreCollections(StarSystemDef definition) =>
        new(
            definition.SystemShopItems ?? [],
            definition.FactionShopItems ?? [],
            definition.BlackMarketShopItems ?? []);

    // The query takes the tag sets and biomes as sets (SQL IN), so their order doesn't change its result.
    private static List<DefinitionReference> ReadPlayableMaps(
        StarSystemDef definition,
        Dictionary<string, List<DefinitionReference>> playableMaps)
    {
        var queryInput = string.Join(
            "|",
            string.Join(",", definition.MapRequiredTags.OrderBy(tag => tag, StringComparer.Ordinal)),
            string.Join(",", definition.MapExcludedTags.OrderBy(tag => tag, StringComparer.Ordinal)),
            string.Join(
                ",",
                definition.SupportedBiomes
                    .OrderBy(biome => biome)
                    .Select(biome => ((int)biome).ToString(CultureInfo.InvariantCulture))));
        if (!playableMaps.TryGetValue(queryInput, out var maps))
        {
            maps = MapReader.ReadPlayableMaps(definition)
                .Select(map => new DefinitionReference(map.MapID, map.FriendlyName))
                .OrderBy(map => map)
                .ToList();
            playableMaps.Add(queryInput, maps);
        }

        return maps;
    }
}
