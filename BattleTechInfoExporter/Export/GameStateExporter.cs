using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>The single entry point every trigger of the career state export calls.</summary>
internal static class GameStateExporter
{
    private const string GameStateFileName = "game-state.json";
    private const string RulesFileName = "rules.json";

    internal static void Export(SimGameState simGame, ExportTrigger trigger) =>
        CampaignExport.Run(
            simGame,
            trigger,
            () =>
            {
                // Both are read before either is written, so a failing read leaves both files from the same export.
                var gameState = GameStateReader.Read(simGame, trigger);
                var rules = RulesReader.Read(simGame, trigger);
                ExportFileWriter.Write(GameStateFileName, gameState);
                ExportFileWriter.Write(RulesFileName, rules);
            });
}
