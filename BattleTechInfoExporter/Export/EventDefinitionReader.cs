using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;
using HBS.Collections;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the catalog's event definitions from the events the game has loaded, and the references to them.</summary>
internal static class EventDefinitionReader
{
    // The scopes the company's event tracker draws from (SimGameState.CompanyTrackerScope); the pilot and funeral
    // trackers are never rolled (SimGameState.AttemptEvents), and no event has the morale tracker's type.
    private static readonly IReadOnlyList<EventScope> DrawnScopes = [EventScope.Company, EventScope.MechWarrior];

    internal static SortedDictionary<string, EventDefinition> ReadEventDefinitions(DataManager dataManager)
    {
        var definitions = new SortedDictionary<string, EventDefinition>(StringComparer.Ordinal);
        foreach (var definition in dataManager.SimGameEventDefs)
        {
            definitions.Add(definition.Key, ReadEventDefinition(dataManager, definition.Value));
        }

        return definitions;
    }

    // An event a forced event or the career names may be missing from the loaded events; its id stands in for the name.
    internal static DefinitionReference ReferenceTo(DataManager dataManager, string eventId) =>
        dataManager.SimGameEventDefs.TryGet(eventId, out var definition)
            ? DefinitionReferences.ReferenceTo(definition.Description)
            : new DefinitionReference(eventId, eventId);

    // SimGameEventTracker.IsEventValid checks an event's own requirements against the event's company or pilot, and a
    // target's against each candidate; every other requirement is checked against its own scope.
    private static EventDefinition ReadEventDefinition(DataManager dataManager, SimGameEventDef definition) =>
        new(
            definition.Description.Name,
            definition.Scope,
            definition.Weight,
            definition.OneTimeEvent,
            // SimGameEventTracker.GetRandomEvent draws only published events of its type and scopes
            // (MetadataDatabase.GetPublishedQualifyingEventDefs).
            definition.PublishState == SimGameEventDef.EventPublishState.PUBLISHED
            && definition.EventType == SimGameEventDef.SimEventType.NORMAL
            && DrawnScopes.Contains(definition.Scope),
            ReadRequirement(definition.Requirements, definition.Scope),
            ReadRequirementList(definition.AdditionalRequirements),
            (definition.AdditionalObjects ?? [])
            .Select(target => new EventTarget(target.Scope, ReadRequirement(target.Requirements, target.Scope)))
            .ToList(),
            definition.Options.Select(option => ReadOption(dataManager, option)).ToList());

    private static EventOption ReadOption(DataManager dataManager, SimGameEventOption option) =>
        new(
            option.Description.Id,
            option.Description.Name,
            ReadRequirementList(option.RequirementList),
            (option.ResultSets ?? [])
            .Select(outcome => new EventOutcome(
                outcome.Weight,
                (outcome.Results ?? []).Select(result => ReadResult(dataManager, result)).ToList()))
            .ToList());

    private static EventResult ReadResult(DataManager dataManager, SimGameEventResult result) =>
        new(
            result.Scope,
            result.Requirements is { } requirement ? ReadRequirement(requirement, requirement.Scope) : null,
            ReadTags(result.AddedTags),
            ReadTags(result.RemovedTags),
            // The value is already resolved from its constant, e.g. [rep_gain_small].
            (result.Stats ?? []).Select(stat => new CareerStatisticChange(stat.name, stat.ToSingle(), stat.set))
            .ToList(),
            // SimGameState.ApplySimGameEventResult tracks a result as temporary only with a duration.
            result.TemporaryResult && result.ResultDuration > 0 ? result.ResultDuration : null,
            (result.Actions ?? [])
            .Select(action => new ResultAction(
                action.Type,
                string.IsNullOrEmpty(action.value) ? null : action.value))
            .ToList(),
            (result.ForceEvents ?? [])
            .Select(forcedEvent => new ForcedEvent(
                ReferenceTo(dataManager, forcedEvent.EventID),
                forcedEvent.Scope,
                forcedEvent.MinDaysWait,
                forcedEvent.MaxDaysWait,
                forcedEvent.Probability,
                forcedEvent.RetainPilot))
            .ToList());

    // An empty requirement is met by anything (RequirementDef.HasRequirement), so it's left out.
    private static EventRequirement? ReadRequirement(RequirementDef? requirement, EventScope scope) =>
        requirement is null || !requirement.HasRequirement()
            ? null
            : new EventRequirement(
                scope,
                ReadTags(requirement.RequirementTags),
                ReadTags(requirement.ExclusionTags),
                (requirement.RequirementComparisons ?? [])
                .Select(comparison => new StatisticComparison(comparison.obj, comparison.op, comparison.val))
                .ToList());

    private static List<EventRequirement> ReadRequirementList(IEnumerable<RequirementDef>? requirements) =>
        (requirements ?? [])
        .Where(requirement => requirement is not null)
        .Select(requirement => ReadRequirement(requirement, requirement.Scope))
        .OfType<EventRequirement>()
        .ToList();

    private static List<string> ReadTags(TagSet? tags) => tags?.ToList() ?? [];
}
