using System;
using BattleTech;
using BattleTechInfoExporter.Models;
using Contract = BattleTech.Contract;

namespace BattleTechInfoExporter.Export;

/// <summary>The entry points of the mission file, written when the mission ends and again once its salvage is final.</summary>
internal static class MissionExporter
{
    /// <summary>
    ///     Exports the outcome of a contract the game has just completed, before the salvage is chosen, returning it
    ///     for <see cref="ExportSalvageReceived" />, or <c>null</c> where the export was skipped or failed.
    /// </summary>
    internal static MissionOutcome? ExportOutcome(SimGameState simGame, Contract contract)
    {
        MissionOutcome? outcome = null;
        CampaignExport.Run(
            simGame,
            ExportTrigger.MissionCompleted,
            () =>
            {
                var completed = MissionReader.ReadOutcome(simGame, contract);
                completed.Write(MissionOutcome.FileName, ExportTrigger.MissionCompleted);
                outcome = completed;
            });
        return outcome;
    }

    /// <summary>
    ///     Exports the outcome <see cref="ExportOutcome" /> returned again, with the salvage received, once the game
    ///     has finalized the contract's salvage; by now the battle it was read from is being torn down. Fails where
    ///     there is no outcome.
    /// </summary>
    internal static void ExportSalvageReceived(SimGameState simGame, Contract contract, MissionOutcome? outcome) =>
        CampaignExport.Run(
            simGame,
            ExportTrigger.SalvageChosen,
            () => MissionReader.WithSalvageReceived(
                    simGame,
                    contract,
                    outcome
                    ?? throw new InvalidOperationException("The contract's outcome wasn't read when its mission ended"))
                .Write(MissionOutcome.FileName, ExportTrigger.SalvageChosen));
}
