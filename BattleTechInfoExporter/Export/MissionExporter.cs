using BattleTech;
using BattleTechInfoExporter.Models;
using Contract = BattleTech.Contract;
using MissionOutcome = BattleTechInfoExporter.Models.MissionOutcome;

namespace BattleTechInfoExporter.Export;

/// <summary>The entry points of the mission file, written when the mission ends and again once its salvage is final.</summary>
internal static class MissionExporter
{
    private const string MissionOutcomeFileName = "mission-outcome.json";

    // The outcome last exported and its contract. The salvage offer can't be read from the contract once the salvage
    // is final, so the outcome is kept to be written again with the salvage received; the game can't be saved in
    // between, so it's never needed across game starts.
    private static (Contract Contract, MissionOutcome Outcome)? _lastExported;

    /// <summary>Exports the outcome of a contract the game has just completed, before the salvage is chosen.</summary>
    internal static void ExportOutcome(SimGameState simGame, Contract contract) =>
        CampaignExport.Run(
            simGame,
            ExportTrigger.MissionCompleted,
            () =>
            {
                var outcome = MissionReader.ReadOutcome(simGame, contract, ExportTrigger.MissionCompleted);
                _lastExported = (contract, outcome);
                ExportFileWriter.Write(MissionOutcomeFileName, outcome);
            });

    /// <summary>Exports the outcome again with the salvage of the contract once the game has finalized it.</summary>
    internal static void ExportSalvageReceived(SimGameState simGame, Contract contract) =>
        CampaignExport.Run(
            simGame,
            ExportTrigger.SalvageChosen,
            () =>
            {
                if (_lastExported is not { } exported || exported.Contract != contract)
                {
                    ModLog.Logger.LogError(
                        $"Skipped the salvage received of {contract.Name}: its mission outcome wasn't exported");
                    return;
                }

                ExportFileWriter.Write(
                    MissionOutcomeFileName,
                    exported.Outcome with
                    {
                        Trigger = ExportTrigger.SalvageChosen,
                        Salvage = exported.Outcome.Salvage with
                        {
                            Received = MissionReader.ReadSalvageReceived(simGame, contract)
                        }
                    });
            });
}
