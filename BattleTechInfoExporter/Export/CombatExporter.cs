using System;
using System.Diagnostics;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     The entry points of the combat files, the map once the battle begins and its state while it runs, which
///     exist only during a career's battle and are deleted once it is over.
/// </summary>
internal static class CombatExporter
{
    private const string CombatMapFileName = "combat-map.json";
    private const string CombatStateFileName = "combat-state.json";

    /// <summary>Exports the battle's map; a skirmish's, which belongs to no career, is skipped.</summary>
    internal static void ExportMap(CombatGameState combat, ExportTrigger trigger) =>
        Run(
            combat,
            trigger,
            () =>
            {
                // Read on the game's main thread while the battle begins; its time and the pathing groups, which
                // only hold the pathing capabilities loaded by then, are logged to check them in the game.
                var stopwatch = Stopwatch.StartNew();
                var map = CombatMapReader.Read(combat);
                ModLog.Logger.Log(
                    $"Read {CombatMapFileName} in {stopwatch.ElapsedMilliseconds} ms, pathing groups "
                    + string.Join("; ", map.PathingGroups.Select(pathingIds => string.Join(", ", pathingIds))));
                map.Write(CombatMapFileName, trigger);
            });

    /// <summary>Exports the running battle; a skirmish's, which belongs to no career, is skipped.</summary>
    internal static void Export(CombatGameState combat, ExportTrigger trigger) =>
        Run(
            combat,
            trigger,
            () => CombatStateReader.Read(combat.BattleTechGame.Simulation, combat)
                .Write(CombatStateFileName, trigger));

    /// <summary>Deletes the files once no battle is running, so existing files always describe the running one.</summary>
    internal static void Delete()
    {
        // Runs inside the game's own code, like every export (CampaignExport.Run).
        try
        {
            ExportFile.Delete(CombatMapFileName);
            ExportFile.Delete(CombatStateFileName);
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }

    private static void Run(CombatGameState combat, ExportTrigger trigger, Action export)
    {
        if (combat.ActiveContract.SimGameContract)
        {
            CampaignExport.Run(combat.BattleTechGame.Simulation, trigger, export);
        }
    }
}
