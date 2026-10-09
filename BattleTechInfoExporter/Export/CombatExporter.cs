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
    /// <summary>Exports the battle's map; a skirmish's, which belongs to no career, is skipped.</summary>
    internal static void ExportMap(CombatGameState combat, ExportTrigger trigger) =>
        Run(
            combat,
            trigger,
            () =>
            {
                // Read on the game's main thread while the battle begins, so its time is logged, as are the pathing
                // groups, which hold only the pathing capabilities loaded by then.
                var stopwatch = Stopwatch.StartNew();
                var map = CombatMapReader.Read(combat);
                ModLog.Logger.Log(
                    $"Read {CombatMap.FileName} in {stopwatch.ElapsedMilliseconds} ms, pathing groups "
                    + string.Join("; ", map.PathingGroups.Select(pathingIds => string.Join(", ", pathingIds))));
                map.Write(CombatMap.FileName, trigger);
            });

    /// <summary>Exports the running battle; a skirmish's, which belongs to no career, is skipped.</summary>
    internal static void Export(CombatGameState combat, ExportTrigger trigger) =>
        Run(
            combat,
            trigger,
            () =>
            {
                // Read on the game's main thread, so its time is logged: the units' movement runs path and line of
                // fire checks per hex.
                var stopwatch = Stopwatch.StartNew();
                var state = CombatStateReader.Read(combat.BattleTechGame.Simulation, combat);
                ModLog.Logger.Log($"Read {CombatState.FileName} in {stopwatch.ElapsedMilliseconds} ms");
                state.Write(CombatState.FileName, trigger);
            });

    /// <summary>Deletes the files once no battle is running, so existing files always describe the running one.</summary>
    internal static void Delete()
    {
        // Runs inside the game's own code, like every export (CampaignExport.Run).
        try
        {
            ExportFile.Delete(CombatMap.FileName);
            ExportFile.Delete(CombatState.FileName);
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
