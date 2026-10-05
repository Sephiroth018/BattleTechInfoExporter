using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the current system's stores from the game's career state.</summary>
internal static class StoreReader
{
    // A store exists when it has stock (StarSystem.HasFactionStore, HasBlackMarketStore). CanUseSystemStore doesn't
    // check it, the other two do.
    internal static Stores ReadStores(SimGameState simGame, ComponentReferences componentDefinitions)
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
            isUsable ? ReadStore(simGame, componentDefinitions, shop, priceFaction) : null;
    }

    // The price faction is the one Shop.GetPrice takes the reputation of.
    private static Store ReadStore(
        SimGameState simGame,
        ComponentReferences componentDefinitions,
        Shop shop,
        FactionValue priceFaction)
    {
        if (shop.IsPending)
        {
            ModLog.Logger.LogWarning(
                $"The {shop.ThisShopType} store is still generating its stock; it may be incomplete");
        }

        var items = shop.ActiveInventory.Where(item => IsKnownType(shop, item)).ToList();
        return new Store(
            DefinitionReferences.ReferenceTo(priceFaction),
            items
                .Where(item => item.Type != ShopItemType.MechPart)
                .Select(item => componentDefinitions.TryReferenceTo(
                    SimGameState.ComponentTypeToBattleTechResourceType(Shop.ShopItemTypeToComponentType(item.Type)),
                    item.ID) is { } component
                    ? new ComponentForSale(component, CountOf(item), PriceOf(shop, item))
                    : Skip<ComponentForSale>(shop, item))
                .OfType<ComponentForSale>()
                .OrderByComponent(component => component.Component)
                .ToList(),
            items
                .Where(item => item.Type == ShopItemType.MechPart)
                .Select(item => MechReader.TryReferenceToMech(simGame.DataManager, item.ID) is { } mech
                    ? new MechPartsForSale(mech, CountOf(item), PriceOf(shop, item))
                    : Skip<MechPartsForSale>(shop, item))
                .OfType<MechPartsForSale>()
                .OrderByDefinition(parts => parts.Mech)
                .ToList());
    }

    // Stores sell components and mech parts; other types only appear in the list of things to sell to them.
    private static bool IsKnownType(Shop shop, ShopDefItem item)
    {
        if (item.Type is ShopItemType.Weapon or ShopItemType.AmmunitionBox or ShopItemType.HeatSink
            or ShopItemType.JumpJet or ShopItemType.Upgrade or ShopItemType.MechPart)
        {
            return true;
        }

        ModLog.Logger.LogWarning($"Skipped {item.ID} in the {shop.ThisShopType} store: unexpected type {item.Type}");
        return false;
    }

    // Shop.GetPrice can't price an item without a definition, so the item is left out instead of referenced by id.
    private static T? Skip<T>(Shop shop, ShopDefItem item) where T : class
    {
        ModLog.Logger.LogWarning($"Skipped {item.ID} in the {shop.ThisShopType} store: no {item.Type} definition");
        return null;
    }

    private static int? CountOf(ShopDefItem item) => item.IsInfinite ? null : item.Count;

    // The price every store row and the purchase confirmation show (SG_Shop_Screen).
    private static int PriceOf(Shop shop, ShopDefItem item) =>
        shop.GetPrice(item, Shop.PurchaseType.Normal, shop.ThisShopType);
}
