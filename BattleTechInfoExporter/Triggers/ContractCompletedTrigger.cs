using System;
using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;
using Contract = BattleTech.Contract;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires once a contract's results (funds, reputation, salvage, experience, damage, injuries) are applied,
///     for the contracts the game doesn't autosave after; the others are exported by <see cref="SaveTrigger" />.
/// </summary>
[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.ResolveCompleteContract))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class ContractCompletedTrigger
{
    [HarmonyPrefix]
    private static void RememberCompletedContract(
        [HarmonyArgument("__instance")] SimGameState simGame,
        [HarmonyArgument("__state")] out Contract? completedContract)
    {
        completedContract = simGame.CompletedContract;
    }

    [HarmonyPostfix]
    private static void OnContractResolved(
        [HarmonyArgument("__instance")] SimGameState simGame,
        [HarmonyArgument("__state")] Contract? completedContract)
    {
        try
        {
            // The game calls the method every frame until it clears the contract, so this passes once per contract.
            if (completedContract == null || simGame.CompletedContract != null)
            {
                return;
            }

            // Mirrors the game's condition for its SIM_GAME_COMPLETED_CONTRACT autosave. A contract whose results
            // are ignored returns before reaching it, with ResultsResolved still false.
            var isAutosaved = completedContract.ResultsResolved
                              && simGame.PendingMilestoneContract == null
                              && !completedContract.IsFlashpointContract;
            if (!isAutosaved)
            {
                GameStateExporter.Export(simGame, ExportTrigger.ContractCompleted);
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
