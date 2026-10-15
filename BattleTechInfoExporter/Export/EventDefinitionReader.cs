using System;
using System.Collections.Generic;
using System.Globalization;
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

    internal static SortedDictionary<string, EventDefinition> ReadEventDefinitions(DataManager dataManager) =>
        CatalogEntries.ById(
            dataManager.SimGameEventDefs.Select(definition => ReadEventDefinition(dataManager, definition.Value)));

    // An event a forced event or the career names may be missing from the loaded events; its id stands in for the name.
    internal static DefinitionReference ReferenceTo(DataManager dataManager, string eventId) =>
        dataManager.SimGameEventDefs.TryGet(eventId, out var definition)
            ? new DefinitionReference(definition.Description.Id, NameOf(definition))
            : new DefinitionReference(eventId, eventId);

    // Plain, as the catalog entry's name, so a reference's name matches its entry's.
    private static string NameOf(SimGameEventDef definition) => GameText.ToPlainText(definition.Description.Name);

    // SimGameEventTracker.IsEventValid checks an event's own requirements against the event's company or pilot, and a
    // target's against each candidate; every other requirement is checked against its own scope.
    private static EventDefinition ReadEventDefinition(DataManager dataManager, SimGameEventDef definition) =>
        new(
            definition.Description.Id,
            NameOf(definition),
            definition.Scope,
            definition.Weight,
            definition.OneTimeEvent,
            // SimGameEventTracker.GetRandomEvent draws only published events of its type and scopes
            // (MetadataDatabase.GetPublishedQualifyingEventDefs).
            definition.PublishState == SimGameEventDef.EventPublishState.PUBLISHED
            && definition.EventType == SimGameEventDef.SimEventType.NORMAL
            && DrawnScopes.Contains(definition.Scope),
            ReadRequirement(dataManager, definition.Requirements, definition.Scope),
            ReadRequirementList(dataManager, definition.AdditionalRequirements),
            (definition.AdditionalObjects ?? [])
            .Select(target => new EventTarget(
                target.Scope,
                ReadRequirement(dataManager, target.Requirements, target.Scope)))
            .ToList(),
            definition.Options.Select(option => ReadOption(dataManager, option)).ToList());

    private static EventOption ReadOption(DataManager dataManager, SimGameEventOption option) =>
        new(
            option.Description.Id,
            GameText.ToPlainText(option.Description.Name),
            ReadRequirementList(dataManager, option.RequirementList),
            (option.ResultSets ?? [])
            .Select(outcome => new EventOutcome(
                outcome.Weight,
                (outcome.Results ?? []).Select(result => ReadResult(dataManager, result)).ToList()))
            .ToList());

    private static EventResult ReadResult(DataManager dataManager, SimGameEventResult result)
    {
        var statisticChanges = (result.Stats ?? [])
            .Select(ReadStatisticChange)
            .OfType<CareerStatisticChange>()
            .Select(change => (Statistic: change, Component: ReadComponentChange(dataManager, result.Scope, change)))
            .ToList();
        return new EventResult(
            result.Scope,
            result.Requirements is { } requirement
                ? ReadRequirement(dataManager, requirement, requirement.Scope)
                : null,
            ReadTags(result.AddedTags),
            ReadTags(result.RemovedTags),
            statisticChanges.Where(change => change.Component is null).Select(change => change.Statistic).ToList(),
            statisticChanges
                .Select(change => change.Component)
                .OfType<ComponentChange>()
                .OrderBy(change => change.Component)
                .ToList(),
            // SimGameState.ApplySimGameEventResult tracks a result as temporary only with a duration.
            result.TemporaryResult && result.ResultDuration > 0 ? result.ResultDuration : null,
            (result.Actions ?? []).Select(action => ReadAction(dataManager, action)).ToList(),
            (result.ForceEvents ?? [])
            .Select(forcedEvent => new ForcedEvent(
                ReferenceTo(dataManager, forcedEvent.EventID),
                forcedEvent.Scope,
                forcedEvent.MinDaysWait,
                forcedEvent.MaxDaysWait,
                forcedEvent.Probability,
                forcedEvent.RetainPilot))
            .ToList());
    }

    // A change that sets the count adds no copies, so it stays a statistic.
    private static ComponentChange? ReadComponentChange(
        DataManager dataManager,
        EventScope scope,
        CareerStatisticChange change) =>
        !change.IsSet && ReadCountedComponent(dataManager, scope, change.Statistic) is { } component
            ? new ComponentChange(component, ReadCount(change.Statistic, change.Value))
            : null;

    // Events give a count as a float. The base game's data has whole numbers only; a fraction would change what a
    // comparison with a whole count passes.
    private static int ReadCount(string statisticName, float value) =>
        value % 1f == 0f
            ? (int)value
            : throw new InvalidOperationException(
                $"Event gives the count {statisticName} the fraction {value.ToString(CultureInfo.InvariantCulture)}");

    // Events change and compare a stored item's count like any other company statistic. A count of damaged copies or
    // of something other than a component stays a statistic.
    private static ComponentReference? ReadCountedComponent(
        DataManager dataManager,
        EventScope scope,
        string statisticName) =>
        scope == EventScope.Company
        && ItemCountStatistic.TryParse(statisticName) is { IsDamaged: false } statistic
        && Enum.TryParse<BattleTechResourceType>(statistic.Type, out var resourceType)
            ? ComponentReferences.FindReferenceTo(dataManager, resourceType, statistic.Id)
            : null;

    // Each kind's value as SimGameState.ApplySimGameEventResult reads it: the kill, dismiss, heal and single injury
    // actions act on the event's pilot and ignore their value. Only the base game's event actions are known.
    private static ResultAction ReadAction(DataManager dataManager, SimGameResultAction action) =>
        action.Type switch
        {
            var type when PilotOnlyAction.Types.Contains(type) => new PilotOnlyAction(type),
            SimGameResultAction.ActionType.MechWarrior_SetTimeout => new SidelinePilotAction(ParseNumber(action)),
            var type when ChangeInjuriesAction.Types.Contains(type) =>
                new ChangeInjuriesAction(type, ParseNumber(action)),
            SimGameResultAction.ActionType.MechWarrior_Callsign => new RenamePilotAction(RequiredValue(action)),
            SimGameResultAction.ActionType.MechWarrior_AddHiring =>
                new AddPilotForHireAction(PilotReader.ReferenceTo(dataManager, RequiredValue(action))),
            SimGameResultAction.ActionType.System_ShowRewards => new RewardAction(RequiredValue(action)),
            SimGameResultAction.ActionType.System_AddFlashpoint => new AddFlashpointAction(RequiredValue(action)),
            _ => throw new InvalidOperationException($"Unknown event action {action.Type} with value {action.value}")
        };

    private static int ParseNumber(SimGameResultAction action) =>
        int.Parse(RequiredValue(action), NumberStyles.Integer, CultureInfo.InvariantCulture);

    // The game skips an action of these kinds without a value; the base game's data always sets it.
    private static string RequiredValue(SimGameResultAction action) =>
        string.IsNullOrEmpty(action.value)
            ? throw new InvalidOperationException($"Event action {action.Type} has no value")
            : action.value;

    // The value is already resolved from its constant, e.g. [rep_gain_small]. A boolean is 1 or 0, as
    // SimGameState.MeetsStatRequirements compares it; a text can't be compared, so it's left out.
    private static CareerStatisticChange? ReadStatisticChange(SimGameStat stat)
    {
        if (stat.Type == typeof(bool))
        {
            return new CareerStatisticChange(stat.name, stat.ToBool() ? 1f : 0f, stat.set);
        }

        if (float.TryParse(stat.value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return new CareerStatisticChange(stat.name, value, stat.set);
        }

        ModLog.Logger.LogWarning($"Left out the change to the text statistic {stat.name} of an event result");
        return null;
    }

    // An empty requirement is met by anything (RequirementDef.HasRequirement), so it's left out.
    private static EventRequirement? ReadRequirement(
        DataManager dataManager,
        RequirementDef? requirement,
        EventScope scope)
    {
        if (requirement is null || !requirement.HasRequirement())
        {
            return null;
        }

        var statisticComparisons = new List<StatisticComparison>();
        var componentComparisons = new List<ComponentComparison>();
        foreach (var comparison in requirement.RequirementComparisons ?? [])
        {
            if (ReadCountedComponent(dataManager, scope, comparison.obj) is { } component)
            {
                componentComparisons.Add(
                    new ComponentComparison(component, comparison.op, ReadCount(comparison.obj, comparison.val)));
            }
            else
            {
                statisticComparisons.Add(new StatisticComparison(comparison.obj, comparison.op, comparison.val));
            }
        }

        return new EventRequirement(
            scope,
            ReadTags(requirement.RequirementTags),
            ReadTags(requirement.ExclusionTags),
            statisticComparisons,
            componentComparisons.OrderBy(comparison => comparison.Component).ToList());
    }

    private static List<EventRequirement> ReadRequirementList(
        DataManager dataManager,
        IEnumerable<RequirementDef>? requirements) =>
        (requirements ?? [])
        .Where(requirement => requirement is not null)
        .Select(requirement => ReadRequirement(dataManager, requirement, requirement.Scope))
        .OfType<EventRequirement>()
        .ToList();

    private static List<string> ReadTags(TagSet? tags) => tags?.ToList() ?? [];
}
