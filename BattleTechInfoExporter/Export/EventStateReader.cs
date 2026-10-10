using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the career's event state from the game's event trackers.</summary>
internal static class EventStateReader
{
    internal static EventState ReadEventState(SimGameState simGame)
    {
        // Only the company's tracker draws events; the special trackers each hold one scheduled event.
        var tracker = simGame.companyEventTracker;
        return new EventState(
            ReferencesTo(simGame, tracker.discardList),
            ReferencesTo(simGame, tracker.usedOneTimeEvents),
            // A saved event the game no longer has, e.g. a removed mod's, isn't loaded (SimGameEventTracker.Hydrate),
            // and one about a pilot who left the roster is dropped when it comes (OnSuccessfulEventRoll).
            simGame.specialEventTracker
                .Where(scheduled => scheduled.predefinedEvent is not null
                                    && (scheduled.predefinedPilot is null
                                        || simGame.PilotRoster.Contains(scheduled.predefinedPilot)))
                .Select(scheduled => ReadScheduledEvent(simGame, scheduled))
                .ToList());
    }

    private static List<DefinitionReference> ReferencesTo(SimGameState simGame, IEnumerable<string> eventIds) =>
        eventIds.Select(eventId => EventDefinitionReader.ReferenceTo(simGame.DataManager, eventId)).ToList();

    private static ScheduledEvent ReadScheduledEvent(SimGameState simGame, SimGameEventTracker scheduled)
    {
        // SimGameEventTracker.CheckRoll counts a roll before comparing it with the wait, so the next roll is the
        // first of the days left.
        int DayOfRoll(int rollsWaited) => simGame.DaysPassed + Math.Max(1, rollsWaited - scheduled.daysElapsed);

        return new ScheduledEvent(
            DefinitionReferences.ReferenceTo(scheduled.predefinedEvent.Description),
            scheduled.predefinedPilot is { } pilot ? PilotReader.ReferenceTo(pilot) : null,
            DayOfRoll(scheduled.minimumDays),
            DayOfRoll(scheduled.maximumDays),
            (int)scheduled.baseProbability);
    }
}
