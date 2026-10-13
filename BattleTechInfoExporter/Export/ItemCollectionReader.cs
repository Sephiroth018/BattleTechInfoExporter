using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds the catalog's item collections from the collections the game has loaded, all of them at the career's
///     start (SimGameState.RequestDataManagerResources).
/// </summary>
internal static class ItemCollectionReader
{
    internal static SortedDictionary<string, ItemCollectionDefinition> ReadItemCollectionDefinitions(
        DataManager dataManager) =>
        CatalogEntries.ById(
            dataManager.ItemCollectionDefs.Select(collection => ReadItemCollection(dataManager, collection.Value)));

    // What a draw yields (ItemCollectionResultGenerator.GenerateItemCollection): an item, another collection drawn in
    // turn, or nothing for an entry of type None, which still uses up the draw.
    private static ItemCollectionDefinition ReadItemCollection(DataManager dataManager, ItemCollectionDef collection)
    {
        var entries = collection.Entries.Where(entry => IsKnownType(collection, entry)).ToList();
        return new ItemCollectionDefinition(
            collection.ID,
            ReferencedEntries.Read(
                    entries.Where(entry => ShopItemReferences.IsComponent(entry.Type)),
                    entry => ShopItemReferences.TryReferenceToComponent(dataManager, entry.Type, entry.ID),
                    (component, entry) => new ComponentCollectionEntry(component, CountOf(entry), entry.Weight))
                .OrderBy(component => component.Component)
                .ToList(),
            ReferencedEntries.Read(
                    entries.Where(entry => entry.Type == ShopItemType.Mech),
                    entry => MechReader.TryReferenceToMech(dataManager, entry.ID),
                    (mech, entry) => new MechCollectionEntry(mech, CountOf(entry), entry.Weight))
                .OrderBy(mech => mech.Mech)
                .ToList(),
            ReferencedEntries.Read(
                    entries.Where(entry => entry.Type == ShopItemType.MechPart),
                    entry => MechReader.TryReferenceToMech(dataManager, entry.ID),
                    (mech, entry) => new MechPartsCollectionEntry(mech, CountOf(entry), entry.Weight))
                .OrderBy(parts => parts.Mech)
                .ToList(),
            entries
                .Where(entry => entry.Type == ShopItemType.Reference && IsLoaded(dataManager, collection, entry))
                .Select(entry => new NestedCollectionEntry(entry.ID, CountOf(entry), entry.Weight))
                .OrderBy(nested => nested.Collection, StringComparer.Ordinal)
                .ToList(),
            entries.Where(entry => entry.Type == ShopItemType.None).Sum(entry => entry.Weight));
    }

    // Collections hold items, other collections and empty entries; the deprecated chassis type no longer occurs.
    private static bool IsKnownType(ItemCollectionDef collection, ItemCollectionDef.Entry entry)
    {
        if (ShopItemReferences.IsComponent(entry.Type)
            || entry.Type is ShopItemType.Mech or ShopItemType.MechPart or ShopItemType.Reference
                or ShopItemType.None)
        {
            return true;
        }

        ModLog.Logger.LogWarning($"Skipped {entry.ID} in {collection.ID}: unexpected type {entry.Type}");
        return false;
    }

    // Every collection is loaded at the career's start; one that isn't would be loaded when first drawn
    // (ItemCollectionResultGenerator.ProcessQueuedReferenceCollections).
    private static bool IsLoaded(DataManager dataManager, ItemCollectionDef collection, ItemCollectionDef.Entry entry)
    {
        if (dataManager.ItemCollectionDefs.Exists(entry.ID))
        {
            return true;
        }

        ModLog.Logger.LogWarning($"Left out {entry.ID} in {collection.ID}: no item collection definition");
        return false;
    }

    // A count of 0 means unlimited copies of an item (ShopDefItem(ItemCollectionDef.Entry)) and every entry once
    // of a collection (ItemCollectionResultGenerator.GenerateItemCollection).
    private static int? CountOf(ItemCollectionDef.Entry entry) => entry.Count <= 0 ? null : entry.Count;
}
