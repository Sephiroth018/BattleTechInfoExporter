using System;
using BattleTech;
using BattleTech.UI;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when the mech bay changes without the game updating the mech lab queue, which
///     <see cref="MechLabQueueUpdatedTrigger" /> covers.
/// </summary>
[HarmonyPatch]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class MechBayChangedTrigger
{
    // The game cancels a refit's steps by calling itself again without a refund; only the outer call is the player's.
    [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.CancelWorkOrder))]
    [HarmonyPostfix]
    private static void OnWorkOrderCancelled([HarmonyArgument("__instance")] SimGameState simGame, bool includeRefund)
    {
        try
        {
            if (includeRefund)
            {
                GameStateExporter.Export(simGame, ExportTrigger.MechBayChanged);
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }

    // The game also stores and scraps mechs midway through its own actions (cancelling a mech's readying, placing a
    // new mech), so the player's actions are patched instead.
    [HarmonyPatch(typeof(MechBayPanel), nameof(MechBayPanel.OnUnreadyMech))]
    [HarmonyPatch(typeof(MechBayPanel), nameof(MechBayPanel.OnScrapMech))]
    [HarmonyPatch(typeof(MechBayPanel), nameof(MechBayPanel.OnScrapChassis))]
    [HarmonyPostfix]
    private static void OnMechBayMechStoredOrScrapped([HarmonyArgument("__instance")] MechBayPanel mechBay)
    {
        GameStateExporter.Export(mechBay.Sim, ExportTrigger.MechBayChanged);
    }

    // Shown when a new mech arrives with every bay taken; storing or scrapping makes room for it.
    [HarmonyPatch(typeof(MechPlacementPopup), nameof(MechPlacementPopup.ConfirmStoreMech))]
    [HarmonyPatch(typeof(MechPlacementPopup), nameof(MechPlacementPopup.ConfirmScrapMech))]
    [HarmonyPostfix]
    private static void OnPlacementMechStoredOrScrapped(
        [HarmonyArgument("__instance")] MechPlacementPopup mechPlacement)
    {
        GameStateExporter.Export(mechPlacement.Sim, ExportTrigger.MechBayChanged);
    }

    // Queues the repair of a component in storage without updating the queue.
    [HarmonyPatch(typeof(MechBayPanel), nameof(MechBayPanel.OrderItemRepair))]
    [HarmonyPostfix]
    private static void OnStorageRepairQueued([HarmonyArgument("__instance")] MechBayPanel mechBay)
    {
        GameStateExporter.Export(mechBay.Sim, ExportTrigger.MechBayChanged);
    }

    // The sort buttons reorder the queue directly; the popup records it so it can refresh the mech bay when closed.
    [HarmonyPatch(typeof(TaskManagementWidget), nameof(TaskManagementWidget.OnClosed))]
    [HarmonyPrefix]
    private static void OnTaskManagementClosing([HarmonyArgument("__instance")] TaskManagementWidget taskManagement)
    {
        try
        {
            if (taskManagement.modified)
            {
                GameStateExporter.Export(taskManagement.Sim, ExportTrigger.MechBayChanged);
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
