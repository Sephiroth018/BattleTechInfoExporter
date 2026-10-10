using BattleTech;
using BattleTechInfoExporter.Models;
using Starmap = BattleTechInfoExporter.Models.Starmap;

namespace BattleTechInfoExporter.Export;

/// <summary>The single entry point every trigger of the career state export calls.</summary>
internal static class GameStateExporter
{
    internal static void Export(SimGameState simGame, ExportTrigger trigger) =>
        CampaignExport.Run(
            simGame,
            trigger,
            () =>
            {
                // All are read before any is written, so a failing read leaves all files from the same export.
                var starmap = StarSystemReader.Read(simGame);
                var gameState = GameStateReader.Read(simGame, starmap);
                var rules = RulesReader.Read(simGame);
                gameState.Write(GameState.FileName, trigger);
                rules.Write(Rules.FileName, trigger);
                starmap.Write(Starmap.FileName, trigger);
            });
}
