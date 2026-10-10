using System;
using System.Collections.Generic;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Every export file's model and file name; the mod writes no other files into the exports folder.</summary>
internal static class ExportFiles
{
    internal static IReadOnlyList<(Type Model, string FileName)> All { get; } =
    [
        (typeof(GameState), GameState.FileName),
        (typeof(Rules), Rules.FileName),
        (typeof(Starmap), Starmap.FileName),
        (typeof(MissionOutcome), MissionOutcome.FileName),
        (typeof(CombatState), CombatState.FileName),
        (typeof(CombatMap), CombatMap.FileName),
        (typeof(Catalog), Catalog.FileName)
    ];
}
