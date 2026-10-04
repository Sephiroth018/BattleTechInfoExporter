using System;
using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>Fires once at the end of a day on which work orders completed, with the day fully processed.</summary>
[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.OnDayPassed))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class DayPassedTrigger
{
    [HarmonyPrefix]
    private static void RememberCompletedCount([HarmonyArgument("__state")] out int completedCountBefore)
    {
        completedCountBefore = WorkOrderCompletionRecorder.CompletedCount;
    }

    [HarmonyPostfix]
    private static void OnDayPassed(
        [HarmonyArgument("__instance")] SimGameState simGame,
        [HarmonyArgument("__state")] int completedCountBefore)
    {
        try
        {
            if (WorkOrderCompletionRecorder.CompletedCount != completedCountBefore)
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
