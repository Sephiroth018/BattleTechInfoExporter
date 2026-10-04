using System;
using System.Collections.Generic;
using System.Linq;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>The order of the export's lists of entries that refer to a definition, the same in every section.</summary>
internal static class ReferenceOrder
{
    /// <summary>By component type, then name, with the id breaking ties.</summary>
    internal static IEnumerable<TEntry> OrderByComponent<TEntry>(
        this IEnumerable<TEntry> entries,
        Func<TEntry, ComponentReference> component) =>
        entries
            .OrderBy(entry => component(entry).Type)
            .ThenBy(entry => component(entry).Name, StringComparer.Ordinal)
            .ThenBy(entry => component(entry).Id, StringComparer.Ordinal);

    /// <summary>By name, with the id breaking ties.</summary>
    internal static IEnumerable<TEntry> OrderByDefinition<TEntry>(
        this IEnumerable<TEntry> entries,
        Func<TEntry, DefinitionReference> definition) =>
        entries
            .OrderBy(entry => definition(entry).Name, StringComparer.Ordinal)
            .ThenBy(entry => definition(entry).Id, StringComparer.Ordinal);
}
