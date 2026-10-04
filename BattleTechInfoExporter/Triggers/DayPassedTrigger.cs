using System;
using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires once at the end of a day on which the monthly expenses were paid, with the day fully processed.
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
    private static void RememberPaidMonthCount([HarmonyArgument("__state")] out int paidMonthCountBefore)
    {
        paidMonthCountBefore = _paidMonthCount;
    }

    [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.OnDayPassed))]
    [HarmonyPostfix]
    private static void OnDayPassed(
        [HarmonyArgument("__instance")] SimGameState simGame,
        [HarmonyArgument("__state")] int paidMonthCountBefore)
    {
        try
        {
            if (_paidMonthCount != paidMonthCountBefore)
            {
                GameStateExporter.Export(simGame, ExportTrigger.MonthlyExpensesPaid);
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
