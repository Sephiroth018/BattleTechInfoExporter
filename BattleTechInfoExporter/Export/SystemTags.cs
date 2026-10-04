using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Reads a star system's planet tags.</summary>
internal static class SystemTags
{
    // Mirrors the starmap's system panel (SGSystemViewPopulator, HBSTagView), which shows only the tags the
    // metadata database marks as player-visible, by their friendly name (TagDataStructFetcher.GetItem).
    internal static List<DefinitionReference> ReadVisibleTags(StarSystem system) =>
        system.Tags
            .Select(tag => MetadataDatabase.Instance.GetTagIfExists(tag))
            .Where(tag => tag is { PlayerVisible: true })
            .Select(tag => new DefinitionReference(tag.Name, tag.FriendlyName))
            .ToList();
}
