using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds the career's event state, and the tags and statistics of the company and the pilots that the events'
///     requirements check.
/// </summary>
internal static class EventStateReader
{
    // The scopes whose tags and statistics are a pilot's (SimGameState.GetStatsByScope, SimGameEventTracker.IsEventValid).
    private static readonly IReadOnlyList<EventScope> PilotScopes =
    [
        EventScope.MechWarrior,
        EventScope.SecondaryMechWarrior,
        EventScope.TertiaryMechWarrior,
        EventScope.DeadMechWarrior,
        EventScope.Commander
    ];

    internal static EventState ReadEventState(SimGameState simGame, ComparedStatistics comparedStatistics)
    {
        var dataManager = simGame.DataManager;
        // Only the company's tracker draws events; the special trackers each hold one scheduled event.
        var tracker = simGame.companyEventTracker;
        return new EventState(
            tracker.discardList.Select(eventId => EventDefinitionReader.ReferenceTo(dataManager, eventId)).ToList(),
            tracker.usedOneTimeEvents
                .Select(eventId => EventDefinitionReader.ReferenceTo(dataManager, eventId))
                .ToList(),
            // A saved event the game no longer has, e.g. a removed mod's, isn't loaded (SimGameEventTracker.Hydrate).
            simGame.specialEventTracker
                .Where(scheduled => scheduled.predefinedEvent is not null)
                .Select(scheduled => ReadScheduledEvent(simGame, scheduled))
                .ToList(),
            simGame.CompanyTags.ToList(),
            ReadStatistics(simGame.CompanyStats, comparedStatistics.Company));
    }

    /// <summary>The statistics the events' requirements compare, by the scope that holds them.</summary>
    internal static ComparedStatistics ReadComparedStatistics(SimGameState simGame)
    {
        var requirements = EventDefinitionReader.ReadAllRequirements(simGame.DataManager).ToList();
        return new ComparedStatistics(
            ReadComparedStatisticNames(requirements, scope => scope == EventScope.Company),
            ReadComparedStatisticNames(requirements, PilotScopes.Contains));
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

    private static List<string> ReadComparedStatisticNames(
        IEnumerable<(EventScope Scope, RequirementDef Requirement)> requirements,
        Func<EventScope, bool> isInScope) =>
        requirements
            .Where(entry => isInScope(entry.Scope))
            .SelectMany(entry => entry.Requirement.RequirementComparisons ?? [])
            .Select(comparison => comparison.obj)
            .Distinct(StringComparer.Ordinal)
            .ToList();

    // SimGameEventTracker.CheckRoll counts a roll before comparing it with the wait, so the next roll is the first of
    // the days left; the roll on the last day always brings the event.
    private static ScheduledEvent ReadScheduledEvent(SimGameState simGame, SimGameEventTracker scheduled) =>
        new(
            EventDefinitionReader.ReferenceTo(simGame.DataManager, scheduled.predefinedEvent.Description.Id),
            scheduled.predefinedPilot is { } pilot ? PilotReader.ReferenceTo(pilot) : null,
            simGame.DaysPassed + Math.Max(1, scheduled.minimumDays - scheduled.daysElapsed),
            simGame.DaysPassed + Math.Max(1, scheduled.maximumDays - scheduled.daysElapsed),
            (int)scheduled.baseProbability);

    /// <param name="Company">The company's statistics some requirement compares.</param>
    /// <param name="Pilot">The pilots' statistics some requirement compares, the commander's included.</param>
    internal sealed record ComparedStatistics(IReadOnlyList<string> Company, IReadOnlyList<string> Pilot);
}
