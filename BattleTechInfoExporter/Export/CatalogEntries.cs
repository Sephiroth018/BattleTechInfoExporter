using System;
using System.Collections.Generic;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the catalog's root objects, keyed by their entries' own ids so a key and its entry's id are one value.</summary>
internal static class CatalogEntries
{
    internal static SortedDictionary<string, TEntry> ById<TEntry>(IEnumerable<TEntry> entries)
        where TEntry : ICatalogEntry
    {
        var byId = new SortedDictionary<string, TEntry>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            byId.Add(entry.Id, entry);
        }

        return byId;
    }
}
