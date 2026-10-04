using System;
using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires once at the end of a day on which the monthly expenses were paid or work orders completed, with the day
///     fully processed.
/// </summary>
[HarmonyPatch]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class DayPassedTrigger
{
    // Only ever increases, so a count taken before a day tells whether something happened during it.
    private static int _paidMonthCount;

    // The game calls a month a quarter: SimGameState.DeductQuarterlyFunds runs every 30 days.
    [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.DeductQuarterlyFunds))]
    [HarmonyPostfix]
    private static void RecordMonthlyExpensesPaid()
    {
        _paidMonthCount++;
    }

    [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.OnDayPassed))]
    [HarmonyPrefix]
    private static void RememberCounts(
        [HarmonyArgument("__state")] out (int PaidMonths, int CompletedWorkOrders) countsBefore)
    {
        countsBefore = (_paidMonthCount, WorkOrderCompletionRecorder.CompletedCount);
    }

    [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.OnDayPassed))]
    [HarmonyPostfix]
    private static void OnDayPassed(
        [HarmonyArgument("__instance")] SimGameState simGame,
        [HarmonyArgument("__state")] (int PaidMonths, int CompletedWorkOrders) countsBefore)
    {
        try
        {
            // One export per day; the end of the month is the rarer and bigger change.
            if (_paidMonthCount != countsBefore.PaidMonths)
            {
                GameStateExporter.Export(simGame, ExportTrigger.MonthlyExpensesPaid);
            }
            else if (WorkOrderCompletionRecorder.CompletedCount != countsBefore.CompletedWorkOrders)
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
