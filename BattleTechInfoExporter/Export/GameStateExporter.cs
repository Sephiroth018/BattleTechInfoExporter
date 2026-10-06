using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>The single entry point every trigger of the career state export calls.</summary>
internal static class GameStateExporter
{
    private const string GameStateFileName = "game-state.json";
    private const string RulesFileName = "rules.json";
    private const string StarSystemsFileName = "star-systems.json";

    internal static void Export(SimGameState simGame, ExportTrigger trigger) =>
        CampaignExport.Run(
            simGame,
            trigger,
            () =>
            {
                // All are read before any is written, so a failing read leaves all files from the same export.
                var gameState = GameStateReader.Read(simGame, trigger);
                var rules = RulesReader.Read(simGame, trigger);
                var starmap = StarSystemReader.Read(simGame, trigger);
                ExportFileWriter.Write(GameStateFileName, gameState);
                ExportFileWriter.Write(RulesFileName, rules);
                ExportFileWriter.Write(StarSystemsFileName, starmap);
            });
}
