using BattleTech;
using BattleTechInfoExporter.Models;
using Contract = BattleTech.Contract;

namespace BattleTechInfoExporter.Export;

/// <summary>The entry points of the mission's export files, one per file.</summary>
internal static class MissionExporter
{
    private const string MissionOutcomeFileName = "mission-outcome.json";
    private const string SalvageReceivedFileName = "salvage-received.json";

    /// <summary>
    ///     Exports the outcome of a contract the game has just completed, before the salvage is chosen, and deletes
    ///     the previous mission's salvage.
    /// </summary>
    internal static void ExportOutcome(SimGameState simGame, Contract contract) =>
        CampaignExport.Run(
            simGame,
            ExportTrigger.MissionCompleted,
            () =>
            {
                // Deleted only after a successful read and before the write, so a salvage file never sits next to
                // an outcome it doesn't belong to.
                var outcome = MissionReader.ReadOutcome(simGame, contract, ExportTrigger.MissionCompleted);
                ExportFileWriter.Delete(SalvageReceivedFileName);
                ExportFileWriter.Write(MissionOutcomeFileName, outcome);
            });

    /// <summary>Exports the salvage of a contract once the game has finalized it.</summary>
    internal static void ExportSalvageReceived(SimGameState simGame, Contract contract) =>
        CampaignExport.Run(
            simGame,
            ExportTrigger.SalvageChosen,
            () => ExportFileWriter.Write(
                SalvageReceivedFileName,
                MissionReader.ReadSalvageReceived(simGame, contract, ExportTrigger.SalvageChosen)));
}
