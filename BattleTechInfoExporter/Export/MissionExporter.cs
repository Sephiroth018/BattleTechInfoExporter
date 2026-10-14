using System;
using System.Collections.Generic;
using BattleTech;
using BattleTechInfoExporter.Models;
using Contract = BattleTech.Contract;

namespace BattleTechInfoExporter.Export;

/// <summary>The entry points of the mission file, written when the mission ends and again once its salvage is final.</summary>
internal static class MissionExporter
{
    /// <summary>
    ///     Exports the outcome of a contract the game has just completed, before the salvage is chosen, returning its
    ///     objectives for <see cref="ExportSalvageReceived" />, or null where the export was skipped or failed.
    /// </summary>
    internal static IReadOnlyList<ObjectiveResult>? ExportOutcome(SimGameState simGame, Contract contract)
    {
        IReadOnlyList<ObjectiveResult>? objectives = null;
        CampaignExport.Run(
            simGame,
            ExportTrigger.MissionCompleted,
            () =>
            {
                var outcome = MissionReader.ReadOutcome(
                    simGame,
                    contract,
                    new SalvageOffer(contract.GetPotentialSalvage(), contract.SalvageResults),
                    MissionReader.ReadObjectives(contract),
                    null);
                outcome.Write(MissionOutcome.FileName, ExportTrigger.MissionCompleted);
                objectives = outcome.Objectives;
            });
        return objectives;
    }

    /// <summary>
    ///     Exports the outcome again, with the salvage received, once the game has finalized the contract's salvage. The
    ///     objectives are those <see cref="ExportOutcome" /> returned, as the encounter layer that groups them is
    ///     destroyed by now; the export fails where it returned none.
    /// </summary>
    internal static void ExportSalvageReceived(
        SimGameState simGame,
        Contract contract,
        SalvageOffer offer,
        IReadOnlyList<ObjectiveResult>? objectives) =>
        CampaignExport.Run(
            simGame,
            ExportTrigger.SalvageChosen,
            () => MissionReader.ReadOutcome(
                    simGame,
                    contract,
                    offer,
                    objectives
                    ?? throw new InvalidOperationException(
                        "The contract's objectives weren't read when its mission ended"),
                    contract.SalvageResults)
                .Write(MissionOutcome.FileName, ExportTrigger.SalvageChosen));
}
