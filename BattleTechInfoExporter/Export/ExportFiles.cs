using System;
using System.Collections.Generic;
using System.IO;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Every export file's model and file name, and the name of each file's schema; the mod writes no other files
///     into the exports folder and ships no other schemas.
/// </summary>
internal static class ExportFiles
{
    /// <summary>The schemas' folder in the mod's folder, where the build ships them.</summary>
    internal static readonly string SchemaDirectory = Path.Combine(ModAssembly.Directory, "schemas");

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

    /// <summary>The file name of an export file's schema, e.g. <c>game-state.schema.json</c>.</summary>
    internal static string SchemaFileName(string fileName) => Path.ChangeExtension(fileName, ".schema.json");
}
