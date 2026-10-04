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
    internal static Storage ReadStorage(SimGameState simGame, ComponentDefinitionReader componentDefinitions)
    {
        var storedItems = ReadStoredItems(simGame);
        return new Storage(
            ReadEquipment(simGame, componentDefinitions, storedItems),
            ReadMechs(simGame),
            ReadMechParts(simGame, storedItems));
    }

    // Filtered as SimGameState.GetAllInventoryItemDefs does. That method isn't used: it generates a game UID for
    // every component, which changes the career's state.
    private static List<StoredComponent> ReadEquipment(
        SimGameState simGame,
        ComponentDefinitionReader componentDefinitions,
        IReadOnlyList<(string Type, string Id)> storedItems) =>
        storedItems
            .Where(item => item.Type != SimGameState.MECH_PART_ITEM)
            // Working and damaged copies are stats of their own.
            .Distinct()
            .Select(item => (item.Type, item.Id,
                ResourceType: (BattleTechResourceType)Enum.Parse(typeof(BattleTechResourceType), item.Type)))
            .Where(item => item.ResourceType != BattleTechResourceType.MechDef
                           && simGame.DataManager.Exists(item.ResourceType, item.Id))
            .Select(item =>
            {
                var definition = simGame.GetComponentDef(item.ResourceType, item.Id);
                // Counted as MechLabPanel.PopulateInventory does.
                return new StoredComponent(
                    componentDefinitions.ReferenceTo(definition.ComponentType, item.Id, definition),
                    simGame.GetItemCount(item.Id, item.Type, SimGameState.ItemCountType.UNDAMAGED_ONLY),
                    simGame.GetItemCount(item.Id, item.Type, SimGameState.ItemCountType.DAMAGED_ONLY));
            })
            .OrderBy(component => component.Component.Type)
            .ThenBy(component => component.Component.Name, StringComparer.Ordinal)
            .ThenBy(component => component.Component.Id, StringComparer.Ordinal)
            .ToList();

    // A stored mech's stat is named after its chassis id, though its type is MechDef.
    private static List<StoredChassis> ReadMechs(SimGameState simGame) =>
        simGame.GetAllInventoryMechDefs(false)
            .Select(chassis => new StoredChassis(
                MechReader.ReferenceTo(chassis),
                chassis.weightClass,
                chassis.StockRole,
                chassis.Tonnage,
                MechReader.ReadHardpoints(chassis),
                chassis.MaxJumpjets,
                // Counted as MechBayMechStorageWidget.InitInventory does.
                simGame.GetItemCount(chassis.Description, typeof(MechDef), SimGameState.ItemCountType.UNDAMAGED_ONLY)))
            .OrderBy(mech => mech.Chassis.Name, StringComparer.Ordinal)
            .ThenBy(mech => mech.Chassis.Id, StringComparer.Ordinal)
            .ToList();

    // A mech part's stat is named after the mech the parts assemble into (SimGameState.AddMechPart). Not read from
    // SimGameState.GetAllInventoryMechParts, which returns only the chassis.
    private static List<StoredMechParts> ReadMechParts(
        SimGameState simGame,
        IReadOnlyList<(string Type, string Id)> storedItems) =>
        storedItems
            .Where(item => item.Type == SimGameState.MECH_PART_ITEM
                           && simGame.DataManager.Exists(BattleTechResourceType.MechDef, item.Id))
            .Select(item => new StoredMechParts(
                MechReader.ReferenceTo(simGame.DataManager.MechDefs.Get(item.Id)),
                simGame.GetItemCount(item.Id, item.Type, SimGameState.ItemCountType.UNDAMAGED_ONLY)))
            .OrderBy(parts => parts.Mech.Name, StringComparer.Ordinal)
            .ThenBy(parts => parts.Mech.Id, StringComparer.Ordinal)
            .ToList();

    // The stats with a count of at least one, as SimGameState.GetAllInventoryItemDefs and GetAllInventoryMechParts
    // select them.
    private static List<(string Type, string Id)> ReadStoredItems(SimGameState simGame) =>
        simGame.GetAllInventoryStrings()
            .Where(statName => simGame.CompanyStats.GetValue<int>(statName) >= 1)
            .Select(statName =>
            {
                var parts = statName.Split('.');
                return (Type: parts[1], Id: parts[2]);
            })
            .ToList();
}
