using System;
using System.Collections.Generic;
using System.Linq;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the entries of a list that refers to definitions, from the game's items.</summary>
internal static class ReferencedEntries
{
    /// <summary>
    ///     The entry of every item whose definition the reference lookup finds; an item without one is left out, which
    ///     the lookup logs.
    /// </summary>
    internal static IEnumerable<TEntry> Read<TItem, TReference, TEntry>(
        IEnumerable<TItem> items,
        Func<TItem, TReference?> tryReferenceTo,
        Func<TReference, TItem, TEntry> entryOf)
        where TReference : Reference
        where TEntry : class =>
        items
            .Select(item => tryReferenceTo(item) is { } reference ? entryOf(reference, item) : null)
            .OfType<TEntry>();
}
