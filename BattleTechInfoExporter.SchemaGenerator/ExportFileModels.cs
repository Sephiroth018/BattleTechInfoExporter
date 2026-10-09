using System;
using System.Collections.Generic;
using System.Linq;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.SchemaGenerator;

/// <summary>Every export file's model and file name.</summary>
internal static class ExportFileModels
{
    internal static IReadOnlyList<(Type Model, string FileName)> All { get; } = CheckComplete(
    [
        (typeof(GameState), GameState.FileName),
        (typeof(Rules), Rules.FileName),
        (typeof(Starmap), Starmap.FileName),
        (typeof(FinancialReport), FinancialReport.FileName),
        (typeof(MissionOutcome), MissionOutcome.FileName),
        (typeof(CombatState), CombatState.FileName),
        (typeof(CombatMap), CombatMap.FileName),
        (typeof(Catalog), Catalog.FileName)
    ]);

    // A file model missing from the list would ship without a schema.
    private static IReadOnlyList<(Type Model, string FileName)> CheckComplete(
        IReadOnlyList<(Type Model, string FileName)> files)
    {
        var unlisted = typeof(ExportFile).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false } && type.IsSubclassOf(typeof(ExportFile)))
            .Except(files.Select(file => file.Model))
            .ToList();
        return unlisted.Count == 0
            ? files
            : throw new InvalidOperationException(
                $"Export file models without a schema: {string.Join(", ", unlisted.Select(type => type.Name))}");
    }
}
