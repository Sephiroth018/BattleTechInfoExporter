using System.Collections.Generic;
using System.Linq;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>An event the game can show between missions: when it can come, and the options it offers.</summary>
/// <param name="Name">
///     The event's title; placeholders such as <c>{TGT_MW.Callsign}</c> stand for the pilots the game picks.
/// </param>
/// <param name="Scope">
///     What the event is about: the company, or a pilot it picks among those meeting
///     <paramref name="Requirements" /> and without an event timeout.
/// </param>
/// <param name="Weight">The event's draw weight among the events that can come.</param>
/// <param name="IsOneTime">Whether the event comes at most once per career.</param>
/// <param name="CanBeDrawn">
///     Whether the daily event roll can draw the event; when false, only a <see cref="ForcedEvent" /> brings it.
/// </param>
/// <param name="Requirements">
///     What the event's company or pilot (<paramref name="Scope" />) must meet; <c>null</c> when nothing.
/// </param>
/// <param name="AdditionalRequirements">What the other scopes must meet, all of them.</param>
/// <param name="AdditionalTargets">
///     The other pilots or the mech the event involves, each one the game finds meeting its requirements; without
///     one, the event can't come.
/// </param>
/// <param name="Options">The options the event offers, in the order it shows them.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EventDefinition(
    string Name,
    EventScope Scope,
    int Weight,
    bool IsOneTime,
    bool CanBeDrawn,
    EventRequirement? Requirements,
    IReadOnlyList<EventRequirement> AdditionalRequirements,
    IReadOnlyList<EventTarget> AdditionalTargets,
    IReadOnlyList<EventOption> Options)
{
    /// <summary>Every requirement of the event, its targets, its options and their results.</summary>
    internal IEnumerable<EventRequirement> EnumerateRequirements() =>
        new[] { Requirements }
            .Concat(AdditionalTargets.Select(target => target.Requirements))
            .Concat(Options
                .SelectMany(option => option.Outcomes)
                .SelectMany(outcome => outcome.Results)
                .Select(result => result.Requirements))
            .OfType<EventRequirement>()
            .Concat(AdditionalRequirements)
            .Concat(Options.SelectMany(option => option.Requirements));
}

/// <param name="Scope">The second pilot, the third pilot or a mech in the mech bay.</param>
/// <param name="Requirements">What the pilot or mech must meet; <c>null</c> when any will do.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EventTarget(EventScope Scope, EventRequirement? Requirements);

/// <summary>
///     An option the event offers; every option is shown, and one whose requirements aren't met can't be picked.
/// </summary>
/// <param name="Id">The option's id from the game's data files.</param>
/// <param name="Name">The option's text, with placeholders as in <see cref="EventDefinition.Name" />.</param>
/// <param name="Requirements">What must be met to pick the option, all of them.</param>
/// <param name="Outcomes">What picking the option can lead to; the game draws one by weight.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EventOption(
    string Id,
    string Name,
    IReadOnlyList<EventRequirement> Requirements,
    IReadOnlyList<EventOutcome> Outcomes);

/// <param name="Weight">The outcome's draw weight among the option's outcomes.</param>
/// <param name="Results">The changes the outcome makes, in order.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EventOutcome(int Weight, IReadOnlyList<EventResult> Results);

/// <summary>One change an outcome makes to one scope.</summary>
/// <param name="Scope">Whose tags and statistics change, or whom the actions concern.</param>
/// <param name="Requirements">
///     What must be met for the result to apply; <c>null</c> when it always applies.
/// </param>
/// <param name="AddedTags">The tags added.</param>
/// <param name="RemovedTags">The tags removed.</param>
/// <param name="StatisticChanges">
///     The statistics changed; a boolean statistic's value as 1 or 0, as the requirements compare it.
/// </param>
/// <param name="DurationDays">
///     How many days the tag and statistic changes last before the game reverts them; <c>null</c> when they last.
/// </param>
/// <param name="Actions">What else the result does, e.g. kill or dismiss the pilot.</param>
/// <param name="ForcedEvents">The events the result schedules.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EventResult(
    EventScope Scope,
    EventRequirement? Requirements,
    IReadOnlyList<string> AddedTags,
    IReadOnlyList<string> RemovedTags,
    IReadOnlyList<CareerStatisticChange> StatisticChanges,
    int? DurationDays,
    IReadOnlyList<ResultAction> Actions,
    IReadOnlyList<ForcedEvent> ForcedEvents);

/// <param name="Type">What the action does, e.g. <c>MechWarrior_SetTimeout</c> or <c>MechWarrior_Kill</c>.</param>
/// <param name="Value">
///     What the action needs, as the game's data gives it, e.g. the timeout's days or the hired pilot's id;
///     <c>null</c> when it needs nothing.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ResultAction(SimGameResultAction.ActionType Type, string? Value);

/// <summary>
///     An event a result schedules: from the daily roll <paramref name="MinDaysWait" /> on, each roll brings it with
///     <paramref name="Chance" />, and roll <paramref name="MaxDaysWait" /> always does.
/// </summary>
/// <param name="Event">The event scheduled.</param>
/// <param name="Scope">What the scheduled event is about.</param>
/// <param name="MinDaysWait">The first daily roll, counted from the next, that can bring the event.</param>
/// <param name="MaxDaysWait">The daily roll, counted from the next, that brings the event for sure.</param>
/// <param name="Chance">The chance per daily roll, in percent.</param>
/// <param name="KeepsPilot">Whether the scheduled event is about the same pilot as the result.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ForcedEvent(
    DefinitionReference Event,
    EventScope Scope,
    int MinDaysWait,
    int MaxDaysWait,
    int Chance,
    bool KeepsPilot);
