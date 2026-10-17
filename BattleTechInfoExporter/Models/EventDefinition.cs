using System.Collections.Generic;
using System.Linq;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>An event the game can show between missions: when it can come, and the options it offers.</summary>
/// <param name="Name">
///     The event's title as plain text. Placeholders in braces stand for what the game picks, by its prefix:
///     <c>TGT_MW</c> the event's pilot, <c>SCN_MW</c> and <c>TRT_MW</c> the second and third pilot,
///     <c>COMMANDER</c> the commander, <c>SCN_UNIT</c> the mech and <c>TGT_SYSTEM</c> the current star system;
///     after the dot what of it is shown, e.g. <c>{TGT_MW.Callsign}</c>, the pronoun <c>{TGT_MW.Subj}</c> or
///     <c>{TGT_SYSTEM.Name}</c>. A switch on a pilot's gender is resolved to its default wording.
/// </param>
/// <param name="Scope">
///     What the event is about: the company, or a pilot it picks among those meeting
///     <paramref name="Requirements" /> and without an event timeout.
/// </param>
/// <param name="Weight">The event's draw weight among the events that can come.</param>
/// <param name="IsOneTime">Whether the event comes at most once per career.</param>
/// <param name="CanBeDrawn">
///     Whether the daily event roll can draw the event; when false, it comes only when another event's result
///     schedules it.
/// </param>
/// <param name="Requirements">
///     What the event's company or pilot (<paramref name="Scope" />) must meet; <c>null</c> when any will
///     do.
/// </param>
/// <param name="AdditionalRequirements">What the other scopes must meet, all of them.</param>
/// <param name="AdditionalTargets">
///     The other pilots or the mech the event involves, each one the game finds meeting its requirements; without
///     one, the event can't come.
/// </param>
/// <param name="Options">The options the event offers, in the order it shows them.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EventDefinition(
    string Id,
    string Name,
    EventScope Scope,
    int Weight,
    bool IsOneTime,
    bool CanBeDrawn,
    EventRequirement? Requirements,
    IReadOnlyList<EventRequirement> AdditionalRequirements,
    IReadOnlyList<EventTarget> AdditionalTargets,
    IReadOnlyList<EventOption> Options) : ICatalogEntry
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

/// <summary>Another pilot or a mech the event involves, besides its own company or pilot.</summary>
/// <param name="Scope">The second pilot, the third pilot or a mech in the mech bay.</param>
/// <param name="Requirements">What the pilot or mech must meet; <c>null</c> when any will do.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EventTarget(EventScope Scope, EventRequirement? Requirements);

/// <summary>
///     An option the event offers; every option is shown, and one whose requirements aren't met can't be picked.
/// </summary>
/// <param name="Id">
///     The option's id from the game's data files, e.g. <c>outcome_1</c>: the game names options so, not the
///     outcomes.
/// </param>
/// <param name="Name">The option's text, with placeholders as in <see cref="EventDefinition.Name" />.</param>
/// <param name="Requirements">What must be met to pick the option, all of them.</param>
/// <param name="Outcomes">What picking the option can lead to; the game draws one by weight.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EventOption(
    string Id,
    string Name,
    IReadOnlyList<EventRequirement> Requirements,
    IReadOnlyList<EventOutcome> Outcomes);

/// <summary>One of the outcomes an option can lead to, drawn by weight.</summary>
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
///     The statistics changed, apart from the stored components' counts in <paramref name="ComponentChanges" />; a
///     boolean statistic's value as 1 or 0, as the requirements compare it.
/// </param>
/// <param name="ComponentChanges">The components added to or taken from the company's storage.</param>
/// <param name="DurationDays">
///     How many days the tag, statistic and component changes last before the game reverts them; <c>null</c> when
///     they're permanent.
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
    IReadOnlyList<ComponentChange> ComponentChanges,
    int? DurationDays,
    IReadOnlyList<ResultAction> Actions,
    IReadOnlyList<ForcedEvent> ForcedEvents);

/// <summary>A change to how many copies of a component the company keeps in storage.</summary>
/// <param name="Count">The working copies added; negative for copies taken.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ComponentChange(ComponentReference Component, int Count) : ComponentEntry(Component);

/// <summary>
///     What else a result does, about the event's pilot or the company; each value of <see cref="Type" /> has a
///     shape with the field it needs.
/// </summary>
/// <param name="Type">What the action does, e.g. <c>MechWarrior_SetTimeout</c> or <c>MechWarrior_Kill</c>.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record ResultAction(SimGameResultAction.ActionType Type);

/// <summary>
///     An action that needs only the event's pilot and no value: killing, dismissing or healing them, or adding or
///     removing one injury.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record PilotOnlyAction(SimGameResultAction.ActionType Type) : ResultAction(Type)
{
    internal static IReadOnlyCollection<SimGameResultAction.ActionType> Types { get; } =
    [
        SimGameResultAction.ActionType.MechWarrior_Kill,
        SimGameResultAction.ActionType.MechWarrior_Fire,
        SimGameResultAction.ActionType.MechWarrior_Heal,
        SimGameResultAction.ActionType.MechWarrior_AddInjury,
        SimGameResultAction.ActionType.MechWarrior_SubtractInjury
    ];
}

/// <summary>An action that sidelines the event's pilot for a while.</summary>
/// <param name="Days">How many days the pilot is sidelined for.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SidelinePilotAction(int Days)
    : ResultAction(SimGameResultAction.ActionType.MechWarrior_SetTimeout);

/// <summary>An action that adds injuries to or removes them from the event's pilot.</summary>
/// <param name="Injuries">How many injuries the action adds or removes.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ChangeInjuriesAction(SimGameResultAction.ActionType Type, int Injuries) : ResultAction(Type)
{
    internal static IReadOnlyCollection<SimGameResultAction.ActionType> Types { get; } =
    [
        SimGameResultAction.ActionType.MechWarrior_AddInjuries,
        SimGameResultAction.ActionType.MechWarrior_SubtractInjuries
    ];
}

/// <summary>An action that renames the event's pilot.</summary>
/// <param name="Callsign">The pilot's new callsign, which also becomes their name.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RenamePilotAction(string Callsign)
    : ResultAction(SimGameResultAction.ActionType.MechWarrior_Callsign);

/// <summary>An action that adds a pilot to the current star system's hiring hall.</summary>
/// <param name="Pilot">The pilot added.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record AddPilotForHireAction(PilotReference Pilot)
    : ResultAction(SimGameResultAction.ActionType.MechWarrior_AddHiring);

/// <summary>An action that rewards the company with a draw from an item collection.</summary>
/// <param name="ItemCollection">The collection drawn from, keyed as in <see cref="Catalog.ItemCollectionDefinitions" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RewardAction(string ItemCollection)
    : ResultAction(SimGameResultAction.ActionType.System_ShowRewards);

/// <summary>An action that makes a flashpoint available.</summary>
/// <param name="Flashpoint">The flashpoint's id in the game's data.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record AddFlashpointAction(string Flashpoint)
    : ResultAction(SimGameResultAction.ActionType.System_AddFlashpoint);

/// <summary>
///     An event a result schedules: from the daily roll <paramref name="MinDaysWait" /> on, each roll brings it with
///     <paramref name="ChancePercent" />, and roll <paramref name="MaxDaysWait" /> always does.
/// </summary>
/// <param name="Event">The event scheduled.</param>
/// <param name="Scope">What the scheduled event is about.</param>
/// <param name="MinDaysWait">The first daily roll, counted from the next, that can bring the event.</param>
/// <param name="MaxDaysWait">The daily roll, counted from the next, that brings the event for sure.</param>
/// <param name="ChancePercent">The chance per daily roll, from 0 to 100.</param>
/// <param name="KeepsPilot">Whether the scheduled event is about the same pilot as the result.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ForcedEvent(
    DefinitionReference Event,
    EventScope Scope,
    int MinDaysWait,
    int MaxDaysWait,
    int ChancePercent,
    bool KeepsPilot);
