using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the company's storage from the game's career state.</summary>
/// <remarks>
///     The game keeps storage as company stats named <c>Item.&lt;type&gt;.&lt;id&gt;</c>, with a <c>.DAMAGED</c>
///     suffix for damaged components; their value is the count.
/// </remarks>
internal static class StorageReader
{
    // A stored chassis' stat type; mech parts have their own (SimGameState.MECH_PART_ITEM).
    private const string MechType = nameof(BattleTechResourceType.MechDef);

    internal static Storage ReadStorage(SimGameState simGame)
    {
        var storedItems = ReadStoredItems(simGame);
        return new Storage(
            ReadComponents(simGame.DataManager, storedItems),
            ReadChassis(simGame, storedItems),
            ReadMechParts(simGame, storedItems));
    }

    // Filtered as SimGameState.GetAllInventoryItemDefs does. That method isn't used: it generates a game UID for
    // every component, which changes the career's state. Counted as MechLabPanel.PopulateInventory does.
    private static List<StoredComponent> ReadComponents(
        DataManager dataManager,
        IReadOnlyList<StoredItem> storedItems) =>
        ReferencedEntries.Read(
                storedItems
                    .Where(item => item.Type != SimGameState.MECH_PART_ITEM && item.Type != MechType)
                    // Working and damaged copies are stats of their own.
                    .GroupBy(item => (item.Type, item.Id)),
                copies => ComponentReferences.TryReferenceTo(
                    dataManager,
                    (BattleTechResourceType)Enum.Parse(typeof(BattleTechResourceType), copies.Key.Type),
                    copies.Key.Id),
                (component, copies) => new StoredComponent(
                    component,
                    copies.Where(copy => !copy.IsDamaged).Sum(copy => copy.Count),
                    copies.Where(copy => copy.IsDamaged).Sum(copy => copy.Count)))
            .OrderBy(component => component.Component)
            .ToList();

    // A stored mech's stat is named after its chassis id, though its type is MechDef; selected as
    // SimGameState.GetAllInventoryMechDefs does and counted as MechBayMechStorageWidget.InitInventory does.
    private static List<StoredChassis> ReadChassis(SimGameState simGame, IReadOnlyList<StoredItem> storedItems) =>
        ReferencedEntries.Read(
                storedItems.Where(item => item.Type == MechType && !item.IsDamaged),
                item => MechReader.TryGetChassis(simGame.DataManager, item.Id) is { } chassis
                    ? MechReader.ReferenceTo(chassis)
                    : null,
                (chassis, item) => new StoredChassis(chassis, item.Count))
            .OrderBy(chassis => chassis.Chassis)
            .ToList();

    // A mech part's stat is named after the mech the parts assemble into (SimGameState.AddMechPart). Not read from
    // SimGameState.GetAllInventoryMechParts, which returns only the chassis.
    private static List<StoredMechParts> ReadMechParts(SimGameState simGame, IReadOnlyList<StoredItem> storedItems) =>
        ReferencedEntries.Read(
                storedItems.Where(item => item.Type == SimGameState.MECH_PART_ITEM),
                item => MechReader.TryReferenceToMech(simGame.DataManager, item.Id),
                (mech, item) => new StoredMechParts(mech, item.Count))
            .OrderBy(parts => parts.Mech)
            .ToList();

    // The stats with a count of at least one, as SimGameState.GetAllInventoryItemDefs and GetAllInventoryMechParts
    // select them.
    private static List<StoredItem> ReadStoredItems(SimGameState simGame) =>
        simGame.GetAllInventoryStrings()
            .Select(statName =>
            {
                var parts = statName.Split('.');
                return new StoredItem(
                    parts[1],
                    parts[2],
                    statName.EndsWith(".DAMAGED", StringComparison.Ordinal),
                    simGame.CompanyStats.GetValue<int>(statName));
            })
            .Where(item => item.Count >= 1)
            .ToList();

    private sealed record StoredItem(string Type, string Id, bool IsDamaged, int Count);
}
