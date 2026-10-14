using System;
using System.Collections.Generic;
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
    // The contract last completed and the objectives its export read, until the salvage write takes them.
    private static (Contract Contract, IReadOnlyList<ObjectiveResult> Objectives)? _lastCompleted;

    /// <summary>
    ///     Takes the objectives the export read when the contract's mission ended, or null where it didn't read them,
    ///     and forgets the contract.
    /// </summary>
    internal static IReadOnlyList<ObjectiveResult>? TakeObjectivesOf(Contract contract)
    {
        var objectives = _lastCompleted is { } lastCompleted && ReferenceEquals(lastCompleted.Contract, contract)
            ? lastCompleted.Objectives
            : null;
        _lastCompleted = null;
        return objectives;
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
                    MissionExporter.ExportOutcome(contract.BattleTechGame.Simulation, contract) is { } objectives
                        ? (contract, objectives)
                        : null;
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
