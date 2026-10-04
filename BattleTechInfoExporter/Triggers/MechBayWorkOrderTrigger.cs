using System;
using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when mech lab orders with no cost left complete right away from the mech bay, outside a day passing,
///     which <see cref="DayPassedTrigger" /> covers.
/// </summary>
[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.UpdateMechLabWorkQueue))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class MechBayWorkOrderTrigger
{
    [HarmonyPrefix]
    private static void RememberCompletedCount([HarmonyArgument("__state")] out int completedCountBefore)
    {
        completedCountBefore = WorkOrderCompletionRecorder.CompletedCount;
    }

    [HarmonyPostfix]
    private static void OnMechLabQueueUpdated(
        [HarmonyArgument("__instance")] SimGameState simGame,
        bool passDay,
        [HarmonyArgument("__state")] int completedCountBefore)
    {
        try
        {
            if (!passDay && WorkOrderCompletionRecorder.CompletedCount != completedCountBefore)
            {
                GameStateExporter.Export(simGame, ExportTrigger.WorkOrderCompleted);
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
