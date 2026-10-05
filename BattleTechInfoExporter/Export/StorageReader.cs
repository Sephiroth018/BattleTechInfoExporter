using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the company's storage from the game's career state.</summary>
/// <remarks>
///     The game keeps storage as company stats named <c>Item.&lt;type&gt;.&lt;id&gt;</c>, with a <c>.DAMAGED</c>
///     suffix for damaged components; their value is the count.
/// </remarks>
internal static class StorageReader
{
    internal static Storage ReadStorage(SimGameState simGame, ComponentReferences componentReferences)
    {
        var storedItems = ReadStoredItems(simGame);
        return new Storage(
            ReadComponents(componentReferences, storedItems),
            ReadChassis(simGame, storedItems),
            ReadMechParts(simGame, storedItems));
    }

    // Filtered as SimGameState.GetAllInventoryItemDefs does. That method isn't used: it generates a game UID for
    // every component, which changes the career's state. Counted as MechLabPanel.PopulateInventory does.
    private static List<StoredComponent> ReadComponents(
        ComponentReferences componentReferences,
        IReadOnlyList<StoredItem> storedItems) =>
        storedItems
            .Where(item => item.Type != SimGameState.MECH_PART_ITEM)
            // Working and damaged copies are stats of their own.
            .GroupBy(item => (item.Type, item.Id))
            .Select(copies => (copies.Key.Id,
                ResourceType: (BattleTechResourceType)Enum.Parse(typeof(BattleTechResourceType), copies.Key.Type),
                Copies: copies))
            .Where(item => item.ResourceType != BattleTechResourceType.MechDef)
            .Select(item => componentReferences.TryReferenceTo(item.ResourceType, item.Id) is { } component
                ? new StoredComponent(
                    component,
                    item.Copies.Where(copy => !copy.IsDamaged).Sum(copy => copy.Count),
                    item.Copies.Where(copy => copy.IsDamaged).Sum(copy => copy.Count))
                : Skip<StoredComponent>(item.Id))
            .OfType<StoredComponent>()
            .OrderByComponent(component => component.Component)
            .ToList();

    // A stored mech's stat is named after its chassis id, though its type is MechDef; selected as
    // SimGameState.GetAllInventoryMechDefs does and counted as MechBayMechStorageWidget.InitInventory does.
    private static List<StoredChassis> ReadChassis(SimGameState simGame, IReadOnlyList<StoredItem> storedItems) =>
        storedItems
            .Where(item => item.Type == nameof(BattleTechResourceType.MechDef) && !item.IsDamaged)
            .Select(item => simGame.DataManager.ChassisDefs.TryGet(item.Id, out var chassis)
                ? new StoredChassis(
                    MechReader.ReferenceTo(chassis),
                    chassis.weightClass,
                    chassis.StockRole,
                    chassis.Tonnage,
                    MechReader.ReadHardpoints(chassis),
                    chassis.MaxJumpjets,
                    item.Count)
                : Skip<StoredChassis>(item.Id))
            .OfType<StoredChassis>()
            .OrderByDefinition(chassis => chassis.Chassis)
            .ToList();

    // A mech part's stat is named after the mech the parts assemble into (SimGameState.AddMechPart). Not read from
    // SimGameState.GetAllInventoryMechParts, which returns only the chassis.
    private static List<StoredMechParts> ReadMechParts(SimGameState simGame, IReadOnlyList<StoredItem> storedItems) =>
        storedItems
            .Where(item => item.Type == SimGameState.MECH_PART_ITEM)
            .Select(item => MechReader.TryReferenceToMech(simGame.DataManager, item.Id) is { } mech
                ? new StoredMechParts(mech, item.Count)
                : Skip<StoredMechParts>(item.Id))
            .OfType<StoredMechParts>()
            .OrderByDefinition(parts => parts.Mech)
            .ToList();

    private static T? Skip<T>(string id) where T : class
    {
        ModLog.Logger.LogWarning($"Skipped {id} in storage: no definition");
        return null;
    }

    // The stats with a count of at least one, as SimGameState.GetAllInventoryItemDefs and GetAllInventoryMechParts
    // select them.
    private static List<StoredItem> ReadStoredItems(SimGameState simGame) =>
        simGame.GetAllInventoryStrings()
            .Select(statName => (StatName: statName, Count: simGame.CompanyStats.GetValue<int>(statName)))
            .Where(stat => stat.Count >= 1)
            .Select(stat =>
            {
                var parts = stat.StatName.Split('.');
                return new StoredItem(
                    parts[1],
                    parts[2],
                    stat.StatName.EndsWith(".DAMAGED", StringComparison.Ordinal),
                    stat.Count);
            })
            .ToList();

    private sealed record StoredItem(string Type, string Id, bool IsDamaged, int Count);
}
