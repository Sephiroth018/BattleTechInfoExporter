using System;
using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when the mech lab queue is updated outside a day passing, which <see cref="DayPassedTrigger" /> covers:
///     the game does so after an order is queued, a mech is readied or an order is cancelled from the mech bay, and
///     orders with no cost left complete right away.
/// </summary>
[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.UpdateMechLabWorkQueue))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class MechLabQueueUpdatedTrigger
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
            if (!passDay)
            {
                GameStateExporter.Export(simGame,
                    WorkOrderCompletionRecorder.CompletedSince(completedCountBefore) ?? ExportTrigger.MechBayChanged);
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
