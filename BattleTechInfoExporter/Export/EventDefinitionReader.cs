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
            ? new DefinitionReference(eventId, GameText.ToPlainText(definition.Description.Name))
            : new DefinitionReference(eventId, eventId);

    /// <summary>
    ///     Every event's requirements with the scope the game checks them against: an event's own requirements are
    ///     checked against the event's company or pilot (SimGameEventTracker.IsEventValid), the others against their
    ///     own scope.
    /// </summary>
    internal static IEnumerable<(EventScope Scope, RequirementDef Requirement)> ReadAllRequirements(
        DataManager dataManager) =>
        dataManager.SimGameEventDefs.SelectMany(definition => ReadRequirements(definition.Value));

    private static IEnumerable<(EventScope Scope, RequirementDef Requirement)> ReadRequirements(
        SimGameEventDef definition)
    {
        var otherRequirements = (definition.AdditionalRequirements ?? [])
            .Concat((definition.AdditionalObjects ?? []).Select(target => target.Requirements))
            .Concat(definition.Options.SelectMany(option => option.RequirementList ?? []))
            .Concat(definition.Options
                .SelectMany(option => option.ResultSets ?? [])
                .SelectMany(outcome => outcome.Results ?? [])
                .Select(result => result.Requirements))
            .Where(requirement => requirement is not null)
            .Select(requirement => (requirement.Scope, requirement));
        return definition.Requirements is { } requirements
            ? otherRequirements.Prepend((definition.Scope, requirements))
            : otherRequirements;
    }

    private static EventDefinition ReadEventDefinition(DataManager dataManager, SimGameEventDef definition) =>
        new(
            GameText.ToPlainText(definition.Description.Name),
            definition.Scope,
            definition.Weight,
            definition.OneTimeEvent,
            // SimGameEventTracker.GetRandomEvent draws only published events of its type and scopes
            // (MetadataDatabase.GetPublishedQualifyingEventDefs).
            definition.PublishState == SimGameEventDef.EventPublishState.PUBLISHED
            && definition.EventType == SimGameEventDef.SimEventType.NORMAL
            && DrawnScopes.Contains(definition.Scope),
            ReadRequirement(definition.Requirements),
            ReadRequirementList(definition.AdditionalRequirements),
            (definition.AdditionalObjects ?? [])
            .Select(target => new EventTarget(target.Scope, ReadRequirement(target.Requirements)))
            .ToList(),
            definition.Options.Select(option => ReadOption(dataManager, option)).ToList());

    private static EventOption ReadOption(DataManager dataManager, SimGameEventOption option) =>
        new(
            option.Description.Id,
            GameText.ToPlainText(option.Description.Name),
            ReadRequirementList(option.RequirementList),
            (option.ResultSets ?? [])
            .Select(outcome => new EventOutcome(
                outcome.Weight,
                (outcome.Results ?? []).Select(result => ReadResult(dataManager, result)).ToList()))
            .ToList());

    private static EventResult ReadResult(DataManager dataManager, SimGameEventResult result) =>
        new(
            result.Scope,
            ReadRequirement(result.Requirements),
            ReadTags(result.AddedTags),
            ReadTags(result.RemovedTags),
            (result.Stats ?? []).Select(ReadStatisticChange).ToList(),
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

    // The value is already resolved from its constant, e.g. [rep_gain_small]; SimGameState.SetSimGameStat parses it
    // by the statistic's type.
    private static ResultStatisticChange ReadStatisticChange(SimGameStat stat) =>
        new(
            stat.name,
            stat.Type switch
            {
                { } type when type == typeof(int) => stat.ToInt(),
                { } type when type == typeof(float) => stat.ToSingle(),
                { } type when type == typeof(bool) => stat.ToBool(),
                _ => stat.value
            },
            stat.set);

    // An empty requirement is met by anything (RequirementDef.HasRequirement), so it's left out.
    private static EventRequirement? ReadRequirement(RequirementDef? requirement) =>
        requirement is null || !requirement.HasRequirement()
            ? null
            : new EventRequirement(
                requirement.Scope,
                ReadTags(requirement.RequirementTags),
                ReadTags(requirement.ExclusionTags),
                (requirement.RequirementComparisons ?? [])
                .Select(comparison => new StatisticComparison(comparison.obj, comparison.op, comparison.val))
                .ToList());

    private static List<EventRequirement> ReadRequirementList(IEnumerable<RequirementDef?>? requirements) =>
        (requirements ?? []).Select(ReadRequirement).OfType<EventRequirement>().ToList();

    private static List<string> ReadTags(TagSet? tags) => tags?.ToList() ?? [];
}
