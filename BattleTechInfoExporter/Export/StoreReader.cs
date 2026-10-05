using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the current system's stores from the game's career state.</summary>
internal static class StoreReader
{
    // A store exists when it has stock (StarSystem.HasFactionStore, HasBlackMarketStore). CanUseSystemStore doesn't
    // check it, the other two do.
    internal static Stores ReadStores(SimGameState simGame, ComponentReferences componentReferences)
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
            isUsable ? ReadStore(simGame, componentReferences, shop, priceFaction) : null;
    }

    // The price faction is the one Shop.GetPrice takes the reputation of.
    private static Store ReadStore(
        SimGameState simGame,
        ComponentReferences componentReferences,
        Shop shop,
        FactionValue priceFaction)
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
                .Select(item => componentReferences.TryReferenceTo(
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
                .Select(item => MechReader.TryGetMech(simGame.DataManager, item.ID) is { } mech
                    ? ReadMechForSale(shop, item, mech)
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

    private static MechForSale ReadMechForSale(Shop shop, ShopDefItem item, MechDef mech)
    {
        var chassis = mech.Chassis;
        return new MechForSale(
            MechReader.ReferenceTo(mech),
            chassis.weightClass,
            chassis.Tonnage,
            MechReader.ReadMaxArmor(chassis),
            MechReader.ReadHardpoints(chassis),
            chassis.MaxJumpjets,
            CountOf(item),
            PriceOf(shop, item));
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
