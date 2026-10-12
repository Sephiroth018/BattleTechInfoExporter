using System;
using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;
using StarSystem = BattleTech.StarSystem;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires once at the end of a day on which the ship jumped into its destination system or work orders completed,
///     with the day fully processed.
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
            // A jump moves the ship to the next system on its path (SGTravelManager.HandleNextTravelStep); only the
            // one into the destination matters, whose contracts the export generates.
            if (simGame.CurSystem != dayStart.System && SystemContractGenerator.IsInDestination(simGame))
            {
                GameStateExporter.Export(simGame, ExportTrigger.Jumped);
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
