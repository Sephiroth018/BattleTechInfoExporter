using System;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>The single entry point every trigger calls to export the career state.</summary>
internal static class GameStateExporter
{
    private const string FileName = "game-state.json";

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

            var path = ExportFileWriter.Write(FileName, GameStateReader.Read(simGame, trigger));
            ModLog.Logger.Log($"Exported the game state ({trigger}) to {path}");
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
