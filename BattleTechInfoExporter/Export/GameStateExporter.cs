using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>The single entry point every trigger of the career state export calls.</summary>
internal static class GameStateExporter
{
    private const string GameStateFileName = "game-state.json";
    private const string RulesFileName = "rules.json";
    private const string StarSystemsFileName = "star-systems.json";
    private const string FinancialReportFileName = "financial-report.json";

    internal static void Export(SimGameState simGame, ExportTrigger trigger) =>
        CampaignExport.Run(
            simGame,
            trigger,
            () =>
            {
                // All are read before any is written, so a failing read leaves all files from the same export.
                var gameState = GameStateReader.Read(simGame);
                var rules = RulesReader.Read(simGame);
                var starmap = StarSystemReader.Read(simGame);
                var financialReport = FinancialReportReader.Read(simGame);
                gameState.Write(GameStateFileName, trigger);
                rules.Write(RulesFileName, trigger);
                starmap.Write(StarSystemsFileName, trigger);
                financialReport.Write(FinancialReportFileName, trigger);
            });
}
