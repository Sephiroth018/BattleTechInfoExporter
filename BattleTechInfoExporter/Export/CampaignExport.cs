using System;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>The checks every exporter runs its export inside.</summary>
internal static class CampaignExport
{
    /// <summary>Runs the export in the story campaign only, logging every exception it throws.</summary>
    internal static void Run(SimGameState simGame, ExportTrigger trigger, Action export)
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

            export();
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
