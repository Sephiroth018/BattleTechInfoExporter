using BattleTech.UI;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when the store screen completes a purchase or a sale, once per player action: selling a stack sells
///     each copy through its own game call. A refused purchase or sale exports too, since the store's stock can
///     change without funds moving (Shop.Purchase takes a limited item out of stock before checking the funds);
///     the export writes nothing when nothing changed.
/// </summary>
[HarmonyPatch]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class StoreTradeTrigger
{
    [HarmonyPatch(typeof(SG_Shop_Screen), nameof(SG_Shop_Screen.BuyCurrentSelection))]
    [HarmonyPostfix]
    private static void OnPurchaseCompleted([HarmonyArgument("__instance")] SG_Shop_Screen storeScreen)
    {
        GameStateExporter.Export(storeScreen.simState, ExportTrigger.PurchaseCompleted);
    }

    [HarmonyPatch(typeof(SG_Shop_Screen), nameof(SG_Shop_Screen.SellCurrentSelection))]
    [HarmonyPatch(typeof(SG_Shop_Screen), nameof(SG_Shop_Screen.SoldMultipleItems))]
    [HarmonyPostfix]
    private static void OnSaleCompleted([HarmonyArgument("__instance")] SG_Shop_Screen storeScreen)
    {
        GameStateExporter.Export(storeScreen.simState, ExportTrigger.SaleCompleted);
    }
}
