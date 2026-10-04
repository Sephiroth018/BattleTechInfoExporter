using BattleTech.UI;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when a store closes, from the main screen or the mech lab, since something may have been bought or sold.
/// </summary>
[HarmonyPatch(typeof(SG_Shop_Screen), nameof(SG_Shop_Screen.OnCompleted))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class StoreClosedTrigger
{
    [HarmonyPostfix]
    private static void OnStoreClosed([HarmonyArgument("__instance")] SG_Shop_Screen store)
    {
        GameStateExporter.Export(store.simState, ExportTrigger.StoreClosed);
    }
}
