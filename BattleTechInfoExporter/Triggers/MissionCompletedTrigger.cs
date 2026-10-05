using System;
using BattleTech;
using BattleTechInfoExporter.Export;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when a mission ends, still in combat, once the contract holds its final results and the salvage on
///     offer. It covers the after-action report's paths that skip the salvage screen as well.
/// </summary>
[HarmonyPatch(typeof(Contract), nameof(Contract.CompleteContract))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class MissionCompletedTrigger
{
    // The game only logs an error for a contract that isn't in progress, and changes nothing.
    [HarmonyPrefix]
    private static void RememberWasInProgress(
        [HarmonyArgument("__instance")] Contract contract,
        [HarmonyArgument("__state")] out bool wasInProgress)
    {
        wasInProgress = contract.State == Contract.ContractState.InProgress;
    }

    [HarmonyPostfix]
    private static void OnMissionCompleted(
        [HarmonyArgument("__instance")] Contract contract,
        [HarmonyArgument("__state")] bool wasInProgress)
    {
        try
        {
            // Only a career's contract gets results; a skirmish's returns before them.
            if (wasInProgress && contract.SimGameContract)
            {
                MissionExporter.ExportOutcome(contract.BattleTechGame.Simulation, contract);
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
