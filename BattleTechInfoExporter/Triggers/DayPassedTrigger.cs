using System;
using BattleTech;
using BattleTechInfoExporter.Export;
using HarmonyLib;
using JetBrains.Annotations;
using StarSystem = BattleTech.StarSystem;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires once at the end of a day on which work orders completed, with the day fully processed, unless the ship
///     jumped into its destination system that day: then <see cref="JumpedTrigger" /> exports.
/// </summary>
[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.OnDayPassed))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class DayPassedTrigger
{
    [HarmonyPrefix]
    private static void RememberDayStart(
        [HarmonyArgument("__instance")] SimGameState simGame,
        [HarmonyArgument("__state")] out (StarSystem System, int CompletedCount) dayStart)
    {
        dayStart = (simGame.CurSystem, WorkOrderCompletionRecorder.CompletedCount);
    }

    [HarmonyPostfix]
    private static void OnDayPassed(
        [HarmonyArgument("__instance")] SimGameState simGame,
        [HarmonyArgument("__state")] (StarSystem System, int CompletedCount) dayStart)
    {
        try
        {
            // A jump changes the current system; systems jumped through on the way aren't exported.
            if (simGame.CurSystem != dayStart.System && SystemContractGenerator.IsInDestination(simGame))
            {
                JumpedTrigger.ExportOnceRefreshed(simGame);
            }
            else if (WorkOrderCompletionRecorder.CompletedSince(dayStart.CompletedCount) is { } trigger)
            {
                GameStateExporter.Export(simGame, trigger);
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
