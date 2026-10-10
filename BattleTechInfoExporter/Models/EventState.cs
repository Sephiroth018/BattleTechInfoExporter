using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The career's state of the events: those scheduled, drawn lately and used up.</summary>
/// <param name="RecentlyDrawnEvents">
///     The events the daily roll drew lately; it draws none of them again until no other event can come.
/// </param>
/// <param name="UsedOneTimeEvents">The one-time events that have come, which never come again.</param>
/// <param name="ScheduledEvents">The events results have scheduled, in the order the game rolls them.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EventState(
    IReadOnlyList<DefinitionReference> RecentlyDrawnEvents,
    IReadOnlyList<DefinitionReference> UsedOneTimeEvents,
    IReadOnlyList<ScheduledEvent> ScheduledEvents);

/// <summary>
///     An event a result has scheduled. The days assume a daily roll on every day from the next on; the game skips
///     the roll on the day the ship arrives at a star system and on the days the story moves on.
/// </summary>
/// <param name="Event">The event scheduled.</param>
/// <param name="Pilot">The pilot the event is about; <c>null</c> when the game picks one when it comes.</param>
/// <param name="EarliestOnDay">
///     The first day the event can come, on the scale of <see cref="Company.DaysPassed" />.
/// </param>
/// <param name="LatestOnDay">
///     The day the event comes for sure, on the scale of <see cref="Company.DaysPassed" />.
/// </param>
/// <param name="Chance">The chance per daily roll from <paramref name="EarliestOnDay" /> on, in percent.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ScheduledEvent(
    DefinitionReference Event,
    PilotReference? Pilot,
    int EarliestOnDay,
    int LatestOnDay,
    int Chance);
