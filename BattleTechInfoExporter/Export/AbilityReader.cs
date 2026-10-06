using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the rules file's abilities, with the statistics each one changes, from their definitions.</summary>
internal static class AbilityReader
{
    internal static SkillLevelAbility Read(SimGameState simGame, AbilityDef ability) =>
        new(
            DefinitionReferences.ReferenceTo(ability.Description),
            ability.IsPrimaryAbility,
            GameText.ToPlainText(ability.Description.Details),
            ability.ActivationTime,
            ability.Targeting == AbilityDef.TargetingType.NotSet ? null : ability.Targeting,
            ability.ActivationCooldown > 0 ? ability.ActivationCooldown : null,
            ability.NumberOfUses > 0 ? ability.NumberOfUses : null,
            EffectsOf(simGame, ability)
                // Floatie effects only show text over the unit.
                .Where(effect => effect.effectType == EffectType.StatisticEffect)
                .Select(ReadEffect)
                .ToList());

    // Coolant Vent has no effects of its own: Mech applies CoolantVentEffect when it vents, and AbstractActor
    // CoolantVentCooldownEffect when the venting ends.
    private static IEnumerable<EffectData> EffectsOf(SimGameState simGame, AbilityDef ability)
    {
        if (ability.Targeting != AbilityDef.TargetingType.ConfirmCoolantVent)
        {
            return ability.EffectData;
        }

        var resolution = simGame.CombatConstants.ResolutionConstants;
        return ability.EffectData.Append(resolution.CoolantVentEffect).Append(resolution.CoolantVentCooldownEffect);
    }

    private static AbilityEffect ReadEffect(EffectData effect)
    {
        var statistic = effect.statisticData;
        return new AbilityEffect(
            statistic.statName,
            statistic.operation,
            ReadValue(statistic),
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
