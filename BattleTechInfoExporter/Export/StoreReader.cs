using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the current system's stores from the game's career state.</summary>
internal static class StoreReader
{
    // A store exists when it has stock (StarSystem.HasFactionStore, HasBlackMarketStore). CanUseSystemStore doesn't
    // check it, the other two do.
    internal static Stores ReadStores(SimGameState simGame, ComponentDefinitionReader componentDefinitions)
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
        ComponentDefinitionReader componentDefinitions,
        Shop shop,
        FactionValue priceFaction)
    {
        if (shop.IsPending)
        {
            ModLog.Logger.LogWarning(
                $"The {shop.ThisShopType} store is still generating its stock; it may be incomplete");
        }

        var items = shop.ActiveInventory.Where(item => IsReadable(simGame, shop, item)).ToList();
        return new Store(
            DefinitionReferences.ReferenceTo(priceFaction),
            items
                .Where(item => item.Type != ShopItemType.MechPart)
                .Select(item => new ComponentForSale(
                    componentDefinitions.ReferenceTo(Shop.ShopItemTypeToComponentType(item.Type), item.ID),
                    CountOf(item),
                    PriceOf(shop, item)))
                .OrderByComponent(component => component.Component)
                .ToList(),
            items
                .Where(item => item.Type == ShopItemType.MechPart)
                .Select(item => new MechPartsForSale(
                    MechReader.ReferenceTo(simGame.DataManager.MechDefs.Get(item.ID)),
                    CountOf(item),
                    PriceOf(shop, item)))
                .OrderByDefinition(parts => parts.Mech)
                .ToList());
    }

    // Stores sell components and mech parts; other types only appear in the list of things to sell to them.
    // Shop.GetPrice can't price an item without a definition.
    private static bool IsReadable(SimGameState simGame, Shop shop, ShopDefItem item)
    {
        var resourceType = item.Type switch
        {
            ShopItemType.Weapon or ShopItemType.AmmunitionBox or ShopItemType.HeatSink or ShopItemType.JumpJet
                or ShopItemType.Upgrade => SimGameState.ComponentTypeToBattleTechResourceType(
                    Shop.ShopItemTypeToComponentType(item.Type)),
            ShopItemType.MechPart => BattleTechResourceType.MechDef,
            _ => (BattleTechResourceType?)null
        };
        if (resourceType is null)
        {
            ModLog.Logger.LogWarning(
                $"Skipped {item.ID} in the {shop.ThisShopType} store: unexpected type {item.Type}");
            return false;
        }

        if (!simGame.DataManager.Exists(resourceType.Value, item.ID))
        {
            ModLog.Logger.LogWarning($"Skipped {item.ID} in the {shop.ThisShopType} store: no {item.Type} definition");
            return false;
        }

        return true;
    }

    private static int? CountOf(ShopDefItem item) => item.IsInfinite ? null : item.Count;

    // The price every store row and the purchase confirmation show (SG_Shop_Screen).
    private static int PriceOf(Shop shop, ShopDefItem item) =>
        shop.GetPrice(item, Shop.PurchaseType.Normal, shop.ThisShopType);
}
