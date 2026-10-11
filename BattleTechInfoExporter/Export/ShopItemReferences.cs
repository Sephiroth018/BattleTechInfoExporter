using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Resolves what the stores' stock and the item collections hold by the game's <see cref="ShopItemType" />, which
///     both share: components through <see cref="ComponentReferences" />, mechs and mech parts through
///     <see cref="MechReader" /> by the mech's id.
/// </summary>
internal static class ShopItemReferences
{
    // Shop.ShopItemTypeToComponentType throws for the other types.
    internal static bool IsComponent(ShopItemType type) =>
        type is ShopItemType.Weapon or ShopItemType.AmmunitionBox or ShopItemType.HeatSink or ShopItemType.JumpJet
            or ShopItemType.Upgrade;

    /// <summary>
    ///     The reference to a component of a store or collection item type, or <c>null</c> when its definition is
    ///     missing.
    /// </summary>
    internal static ComponentReference? TryReferenceToComponent(
        DataManager dataManager,
        ShopItemType type,
        string componentId) =>
        ComponentReferences.TryReferenceTo(dataManager, Shop.ShopItemTypeToComponentType(type), componentId);
}
