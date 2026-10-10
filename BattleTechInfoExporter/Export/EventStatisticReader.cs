using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Reads the statistics of the company and the pilots that the events' requirements compare.</summary>
internal static class EventStatisticReader
{
    // The scopes whose statistics are a pilot's (SimGameState.GetStatsByScope, SimGameEventTracker.IsEventValid).
    private static readonly IReadOnlyList<EventScope> PilotScopes =
    [
        EventScope.MechWarrior,
        EventScope.SecondaryMechWarrior,
        EventScope.TertiaryMechWarrior,
        EventScope.DeadMechWarrior,
        EventScope.Commander
    ];

    internal static ComparedStatisticNames ReadComparedStatisticNames(DataManager dataManager)
    {
        var requirements = EventDefinitionReader.ReadEventDefinitions(dataManager).Values
            .SelectMany(definition => definition.EnumerateRequirements())
            .ToList();
        return new ComparedStatisticNames(
            ReadNames(requirements.Where(requirement => requirement.Scope == EventScope.Company)),
            ReadNames(requirements.Where(requirement => PilotScopes.Contains(requirement.Scope))));
    }

    /// <summary>
    ///     The statistics as SimGameState.MeetsStatRequirements compares them: as a number, 0 for one the collection
    ///     doesn't have.
    /// </summary>
    internal static SortedDictionary<string, float> ReadStatistics(
        StatCollection statistics,
        IEnumerable<string> statisticNames)
    {
        var values = new SortedDictionary<string, float>(StringComparer.Ordinal);
        foreach (var name in statisticNames)
        {
            values.Add(
                name,
                statistics.ContainsStatistic(name)
                    ? Convert.ToSingle(statistics.GetStatistic(name).CurrentValue.objVal, CultureInfo.InvariantCulture)
                    : 0f);
        }

        return values;
    }

    private static List<string> ReadNames(IEnumerable<EventRequirement> requirements) =>
        requirements
            .SelectMany(requirement => requirement.Comparisons)
            .Select(comparison => comparison.Statistic)
            .Distinct(StringComparer.Ordinal)
            .ToList();

    /// <param name="Company">The company's statistics some requirement compares.</param>
    /// <param name="Pilot">The pilots' statistics some requirement compares, the commander's included.</param>
    internal sealed record ComparedStatisticNames(IReadOnlyList<string> Company, IReadOnlyList<string> Pilot);
}
