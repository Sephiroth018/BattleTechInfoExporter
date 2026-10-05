using System;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>The single entry point every trigger calls to export the career state.</summary>
internal static class GameStateExporter
{
    private const string GameStateFileName = "game-state.json";
    private const string RulesFileName = "rules.json";

    internal static void Export(SimGameState simGame, ExportTrigger trigger)
    {
        // Triggers run inside the game's own code; an exception escaping here would break it.
        try
        {
            if (!simGame.IsCampaign)
            {
                ModLog.Logger.Log(
                    $"Skipped the export ({trigger}): only the story campaign is supported, not {simGame.SimGameMode}");
                return;
            }

            // Both are read before either is written, so a failing read leaves both files from the same export.
            var gameState = GameStateReader.Read(simGame, trigger);
            var rules = RulesReader.Read(simGame, trigger);
            ExportFileWriter.Write(GameStateFileName, gameState);
            ExportFileWriter.Write(RulesFileName, rules);
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
