using System;
using BattleTech.UI;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when a store closes, from the main screen or the mech lab, since something may have been bought or sold.
/// </summary>
[HarmonyPatch]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class StoreClosedTrigger
{
    // SGRoomManager.ClearRooms leaves every room when the career attaches, also the store room nobody entered.
    [HarmonyPatch(typeof(SGRoomController_Shop), nameof(SGRoomController_Shop.LeaveRoom))]
    [HarmonyPrefix]
    private static void RememberStoreRoomActive(
        [HarmonyArgument("__instance")] SGRoomController_Shop storeRoom,
        [HarmonyArgument("__state")] out bool wasActive)
    {
        wasActive = storeRoom.roomActive;
    }

    [HarmonyPatch(typeof(SGRoomController_Shop), nameof(SGRoomController_Shop.LeaveRoom))]
    [HarmonyPostfix]
    private static void OnStoreRoomLeft(
        [HarmonyArgument("__instance")] SGRoomController_Shop storeRoom,
        [HarmonyArgument("__state")] bool wasActive)
    {
        try
        {
            if (wasActive)
            {
                GameStateExporter.Export(storeRoom.simState, ExportTrigger.StoreClosed);
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }

    [HarmonyPatch(typeof(MechLabPanel), nameof(MechLabPanel.CloseShop))]
    [HarmonyPrefix]
    private static void RememberMechLabStoreOpen(
        [HarmonyArgument("__instance")] MechLabPanel mechLab,
        [HarmonyArgument("__state")] out bool wasOpen)
    {
        wasOpen = mechLab.IsShopOpen();
    }

    [HarmonyPatch(typeof(MechLabPanel), nameof(MechLabPanel.CloseShop))]
    [HarmonyPostfix]
    private static void OnMechLabStoreClosed(
        [HarmonyArgument("__instance")] MechLabPanel mechLab,
        [HarmonyArgument("__state")] bool wasOpen)
    {
        try
        {
            if (wasOpen)
            {
                GameStateExporter.Export(mechLab.sim, ExportTrigger.StoreClosed);
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
