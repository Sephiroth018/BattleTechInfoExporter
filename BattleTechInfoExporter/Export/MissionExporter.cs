using BattleTech;
using BattleTechInfoExporter.Models;
using Contract = BattleTech.Contract;

namespace BattleTechInfoExporter.Export;

/// <summary>The entry points of the mission file, written when the mission ends and again once its salvage is final.</summary>
internal static class MissionExporter
{
    private const string MissionOutcomeFileName = "mission-outcome.json";

    /// <summary>Exports the outcome of a contract the game has just completed, before the salvage is chosen.</summary>
    internal static void ExportOutcome(SimGameState simGame, Contract contract) =>
        CampaignExport.Run(
            simGame,
            ExportTrigger.MissionCompleted,
            () => ExportFileWriter.Write(
                MissionOutcomeFileName,
                MissionReader.ReadOutcome(
                    simGame,
                    contract,
                    ExportTrigger.MissionCompleted,
                    new SalvageOffer(contract.GetPotentialSalvage(), contract.SalvageResults),
                    null)));

    /// <summary>
    ///     Exports the outcome again, with the salvage received, once the game has finalized the contract's salvage.
    /// </summary>
    internal static void ExportSalvageReceived(SimGameState simGame, Contract contract, SalvageOffer offer) =>
        CampaignExport.Run(
            simGame,
            ExportTrigger.SalvageChosen,
            () => ExportFileWriter.Write(
                MissionOutcomeFileName,
                MissionReader.ReadOutcome(
                    simGame,
                    contract,
                    ExportTrigger.SalvageChosen,
                    offer,
                    contract.SalvageResults)));
}
