using System;
using System.Collections.Generic;
using System.Linq;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>The order of the export's lists of entries that refer to a definition, the same in every section.</summary>
internal static class ReferenceOrder
{
    /// <summary>By component type, then name, with the id breaking ties.</summary>
    internal static IEnumerable<TEntry> OrderByComponent<TEntry>(this IEnumerable<TEntry> entries)
        where TEntry : ComponentEntry =>
        entries
            .OrderBy(entry => entry.Component.Type)
            .ThenBy(entry => entry.Component.Name, StringComparer.Ordinal)
            .ThenBy(entry => entry.Component.Id, StringComparer.Ordinal);

    /// <summary>By name, with the id breaking ties.</summary>
    internal static IEnumerable<TEntry> OrderByReference<TEntry>(
        this IEnumerable<TEntry> entries,
        Func<TEntry, Reference> reference) =>
        entries
            .OrderBy(entry => reference(entry).Name, StringComparer.Ordinal)
            .ThenBy(entry => reference(entry).Id, StringComparer.Ordinal);
}
