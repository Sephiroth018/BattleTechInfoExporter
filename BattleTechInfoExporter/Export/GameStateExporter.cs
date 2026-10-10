using System.Diagnostics;
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
                // All are read before any is written, so a failing read leaves all files from the same export. The
                // star systems' read is timed: it runs the game's map query per distinct star system input.
                var stopwatch = Stopwatch.StartNew();
                var starmap = StarSystemReader.Read(simGame);
                ModLog.Logger.Log($"Read {Starmap.FileName} in {stopwatch.ElapsedMilliseconds} ms");
                var gameState = GameStateReader.Read(simGame, starmap);
                var rules = RulesReader.Read(simGame);
                gameState.Write(GameState.FileName, trigger);
                rules.Write(Rules.FileName, trigger);
                starmap.Write(Starmap.FileName, trigger);
            });
}
