using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the statistic changes of abilities and components from their effects.</summary>
internal static class EffectReader
{
    // The game has no constant for BaseInitiative.
    private const string BaseInitiativeStatistic = "BaseInitiative";
    private const string PhaseModifierStatistic = AbstractActorConstants.STAT_PHASEMOD;
    private const string PhaseModifierSelfStatistic = AbstractActorConstants.STAT_PHASEMODSELF;

    // A constants file can leave an effect out, which leaves it null.
    internal static List<StatisticChange> ReadStatisticChanges(IEnumerable<EffectData?> effects) =>
        effects
            .OfType<EffectData>()
            // Floatie effects only show text over the unit.
            .Where(effect => effect.effectType == EffectType.StatisticEffect)
            .Select(ReadStatisticChange)
            .ToList();

    /// <summary>The changes of a single effect the game applies on its own, outside any ability or component.</summary>
    internal static List<StatisticChange> ReadStatisticChanges(EffectData? effect) => ReadStatisticChanges([effect]);

    private static StatisticChange ReadStatisticChange(EffectData effect)
    {
        var statistic = effect.statisticData;
        return new StatisticChange(
            statistic.statName,
            statistic.operation,
            ConvertInitiativeToHud(statistic, ReadValue(statistic)),
            ReadTargetCollection(statistic),
            ReadDuration(effect.durationData),
            effect.targetingData.effectTargetType,
            effect.targetingData.effectTriggerType);
    }

    // The game parses modValue into the type modType names (StatisticEffect.SetupVariant); a type other than
    // these three keeps the game's text.
    private static object ReadValue(StatisticEffectData statistic) =>
        statistic.modType switch
        {
            "System.Boolean" => bool.Parse(statistic.modValue),
            "System.Int32" => int.Parse(statistic.modValue, CultureInfo.InvariantCulture),
            "System.Single" => float.Parse(statistic.modValue, CultureInfo.InvariantCulture),
            _ => statistic.modValue
        };

    // A unit's phase is BaseInitiative plus PhaseModifier (AbstractActor.BaseInitiative), which takes over
    // PhaseModifierSelf when the unit's activation ends (AbstractActor.OnActivationEnd).
    private static object ConvertInitiativeToHud(StatisticEffectData statistic, object value) =>
        (statistic.statName, statistic.operation, value) switch
        {
            (BaseInitiativeStatistic, StatCollection.StatOperation.Set, int phase) =>
                HudInitiative.FromGamePhase(phase),
            (BaseInitiativeStatistic or PhaseModifierStatistic or PhaseModifierSelfStatistic,
                StatCollection.StatOperation.Int_Add or StatCollection.StatOperation.Int_Subtract, int change) =>
                -change,
            (PhaseModifierStatistic or PhaseModifierSelfStatistic, StatCollection.StatOperation.Set, int change) =>
                -change,
            (BaseInitiativeStatistic or PhaseModifierStatistic or PhaseModifierSelfStatistic, _, _) =>
                KeepGameInitiative(statistic, value),
            _ => value
        };

    private static object KeepGameInitiative(StatisticEffectData statistic, object value)
    {
        ModLog.Logger.LogWarning(
            $"Exporting {statistic.operation} {value} on {statistic.statName} on the game's initiative scale");
        return value;
    }

    // The filters as the game's data sets them; EffectManager.GetTargetComponents applies the first one set of
    // sub type, type and category.
    private static EffectTargetCollection? ReadTargetCollection(StatisticEffectData statistic) =>
        statistic.targetCollection == StatisticEffectData.TargetCollection.NotSet
            ? null
            : new EffectTargetCollection(
                statistic.targetCollection,
                statistic.targetWeaponSubType == WeaponSubType.NotSet ? null : statistic.targetWeaponSubType,
                statistic.targetWeaponType == WeaponType.NotSet ? null : statistic.targetWeaponType,
                statistic.TargetWeaponCategoryValue.Is_NotSet ? null : statistic.TargetWeaponCategoryValue.FriendlyName,
                statistic.TargetAmmoCategoryValue.Is_NotSet ? null : statistic.TargetAmmoCategoryValue.FriendlyName);

    // Mirrors the ETimer that Effect picks; an ETimer only counts down from a positive duration.
    private static EffectDuration? ReadDuration(EffectDurationData duration)
    {
        if (duration.duration <= 0)
        {
            return null;
        }

        var unit = duration switch
        {
            { ticksOnActivations: true, useActivationsOfTarget: true } => EffectDurationUnit.TargetActivations,
            { ticksOnActivations: true } => EffectDurationUnit.Activations,
            { ticksOnEndOfRound: true } => EffectDurationUnit.Rounds,
            { ticksOnMovements: true } => EffectDurationUnit.Movements,
            _ => EffectDurationUnit.Phases
        };
        return new EffectDuration(duration.duration, unit);
    }
}
