using System;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     The entry points of the combat state file, written during a career's battle and deleted once it is over.
/// </summary>
internal static class CombatExporter
{
    private const string CombatStateFileName = "combat-state.json";

    /// <summary>Exports the running battle; a skirmish's, which belongs to no career, is skipped.</summary>
    internal static void Export(CombatGameState combat, ExportTrigger trigger)
    {
        if (!combat.ActiveContract.SimGameContract)
        {
            return;
        }

        var simGame = combat.BattleTechGame.Simulation;
        CampaignExport.Run(
            simGame,
            trigger,
            () => ExportFileWriter.Write(CombatStateFileName, CombatStateReader.Read(simGame, combat, trigger)));
    }

    /// <summary>Deletes the file once no battle is running, so an existing file always describes the running one.</summary>
    internal static void Delete()
    {
        // Runs inside the game's own code, like every export (CampaignExport.Run).
        try
        {
            ExportFileWriter.Delete(CombatStateFileName);
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
