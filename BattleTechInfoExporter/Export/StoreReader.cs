using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the current system's stores from the game's career state.</summary>
internal static class StoreReader
{
    // A store exists when it has stock (StarSystem.HasFactionStore, HasBlackMarketStore). CanUseSystemStore doesn't
    // check it, the other two do.
    internal static Stores ReadStores(SimGameState simGame)
    {
        var system = simGame.CurSystem;
        return new Stores(
            ReadStoreIfUsable(
                system.CanUseSystemStore() && !system.SystemShop.IsEmpty,
                system.SystemShop,
                system.Def.OwnerValue),
            ReadStoreIfUsable(system.CanUseFactionStore(), system.FactionShop, system.Def.FactionShopOwnerValue),
            ReadStoreIfUsable(
                system.CanUseBlackMarketStore(),
                system.BlackMarketShop,
                FactionEnumeration.GetAuriganPiratesFactionValue()));

        Store? ReadStoreIfUsable(bool isUsable, Shop shop, FactionValue priceFaction) =>
            isUsable ? ReadStore(simGame, shop, priceFaction) : null;
    }

    // The price faction is the one Shop.GetPrice takes the reputation of.
    private static Store ReadStore(SimGameState simGame, Shop shop, FactionValue priceFaction)
    {
        if (shop.IsPending)
        {
            ModLog.Logger.LogWarning(
                $"The {shop.ThisShopType} store is still generating its stock; it may be incomplete");
        }

        // Shop.GetPrice can't price an item without a definition, so such an item is left out instead of referenced
        // by its id.
        var items = shop.ActiveInventory.Where(item => IsKnownType(shop, item)).ToList();
        return new Store(
            DefinitionReferences.ReferenceTo(priceFaction),
            items
                .Where(item => IsComponent(item.Type))
                .Select(item => ComponentReferences.TryReferenceTo(
                    simGame.DataManager,
                    Shop.ShopItemTypeToComponentType(item.Type),
                    item.ID) is { } component
                    ? new ComponentForSale(component, CountOf(item), PriceOf(shop, item))
                    : null)
                .OfType<ComponentForSale>()
                .OrderByComponent(component => component.Component)
                .ToList(),
            items
                .Where(item => item.Type == ShopItemType.Mech)
                // Bought as SimGameState.AddFromShopDefItem does: the id is the mech's.
                .Select(item => MechReader.TryReferenceToMech(simGame.DataManager, item.ID) is { } mech
                    ? new MechForSale(mech, CountOf(item), PriceOf(shop, item))
                    : null)
                .OfType<MechForSale>()
                .OrderByDefinition(mech => mech.Mech)
                .ToList(),
            items
                .Where(item => item.Type == ShopItemType.MechPart)
                .Select(item => MechReader.TryReferenceToMech(simGame.DataManager, item.ID) is { } mech
                    ? new MechPartsForSale(mech, CountOf(item), PriceOf(shop, item))
                    : null)
                .OfType<MechPartsForSale>()
                .OrderByDefinition(parts => parts.Mech)
                .ToList());
    }

    // Stores sell components, whole mechs and mech parts; other types only appear in the list of things to sell to
    // them.
    private static bool IsKnownType(Shop shop, ShopDefItem item)
    {
        if (IsComponent(item.Type) || item.Type is ShopItemType.Mech or ShopItemType.MechPart)
        {
            return true;
        }

        ModLog.Logger.LogWarning($"Skipped {item.ID} in the {shop.ThisShopType} store: unexpected type {item.Type}");
        return false;
    }

    private static bool IsComponent(ShopItemType type) =>
        type is ShopItemType.Weapon or ShopItemType.AmmunitionBox or ShopItemType.HeatSink or ShopItemType.JumpJet
            or ShopItemType.Upgrade;

    private static int? CountOf(ShopDefItem item) => item.IsInfinite ? null : item.Count;

    // The price every store row and the purchase confirmation show (SG_Shop_Screen).
    private static int PriceOf(Shop shop, ShopDefItem item) =>
        shop.GetPrice(item, Shop.PurchaseType.Normal, shop.ThisShopType);
}
