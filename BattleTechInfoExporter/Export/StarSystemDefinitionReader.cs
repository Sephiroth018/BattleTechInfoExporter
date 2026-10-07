using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds the catalog's star system definitions from every star system definition, also those the career doesn't
///     use and those the story swaps in (SimGameState.SetStarSystemDef), which the career loads all of
///     (SimGameState.RequestDataManagerResources).
/// </summary>
internal static class StarSystemDefinitionReader
{
    /// <summary>Keyed by the description's id, as the star systems file and the references to a star system are.</summary>
    internal static SortedDictionary<string, StarSystemDefinition> ReadStarSystemDefinitions(DataManager dataManager)
    {
        var definitions = new SortedDictionary<string, StarSystemDefinition>(StringComparer.Ordinal);
        foreach (var definition in dataManager.SystemDefs)
        {
            definitions.Add(
                definition.Value.Description.Id,
                ReadStarSystemDefinition(dataManager, definition.Value));
        }

        return definitions;
    }

    // A star system's tags are its definition's, also after a swap (StarSystem.SetNewStarSystemDef).
    private static StarSystemDefinition ReadStarSystemDefinition(DataManager dataManager, StarSystemDef definition) =>
        new(
            definition.Description.Name,
            DefinitionReferences.ReferenceTo(definition.OwnerValue),
            ReadVisibleTags(definition),
            ReadBiomes(dataManager, definition));

    // Mirrors the starmap's system panel (SGSystemViewPopulator, HBSTagView), which shows only the tags the
    // metadata database marks as player-visible, by their friendly name (TagDataStructFetcher.GetItem).
    private static List<DefinitionReference> ReadVisibleTags(StarSystemDef definition) =>
        definition.Tags
            .Select(tag => MetadataDatabase.Instance.GetTagIfExists(tag))
            .Where(tag => tag is { PlayerVisible: true })
            .Select(tag => new DefinitionReference(tag.Name, tag.FriendlyName))
            .ToList();

    // StarSystemDef.SupportedBiomes limits the maps of the star system's contracts
    // (SimGameState.GetSinglePlayerProceduralPlayableMaps).
    private static List<DefinitionReference> ReadBiomes(DataManager dataManager, StarSystemDef definition) =>
        definition.SupportedBiomes
            .Select(biome => DefinitionReferences.ReferenceTo(dataManager, biome))
            .ToList();
}
