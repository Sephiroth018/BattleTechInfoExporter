using System;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;
using Contract = BattleTech.Contract;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when a mission ends, still in combat, once the contract holds its final results and the salvage on
///     offer. It covers the after-action report's paths that skip the salvage screen as well.
/// </summary>
[HarmonyPatch(typeof(Contract), nameof(Contract.CompleteContract))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class MissionCompletedTrigger
{
    // The contract last completed and the outcome its export read, until the salvage write takes it.
    private static (Contract Contract, MissionOutcome Outcome)? _lastCompleted;

    /// <summary>
    ///     Takes the outcome the export read when the contract's mission ended, or <c>null</c> where it didn't read
    ///     one, and forgets the contract.
    /// </summary>
    internal static MissionOutcome? TakeOutcomeOf(Contract contract)
    {
        var outcome = _lastCompleted is { } lastCompleted && ReferenceEquals(lastCompleted.Contract, contract)
            ? lastCompleted.Outcome
            : null;
        _lastCompleted = null;
        return outcome;
    }

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
                _lastCompleted =
                    MissionExporter.ExportOutcome(contract.BattleTechGame.Simulation, contract) is { } outcome
                        ? (contract, outcome)
                        : null;
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
