using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The lance's resolve in combat, which Precision Strike and Vigilance spend (see <see cref="SpiritsLevelCosts" />).
/// </summary>
/// <param name="InspiredAt">At this resolve the lance's mechs get <see cref="InspiredEffects" />.</param>
/// <param name="InspireCost">The resolve each mech spends when inspired.</param>
/// <param name="IsAutoInspired">
///     Whether the whole lance is inspired as soon as the resolve allows, and loses it below.
/// </param>
/// <param name="BaselineGainPerRound">
///     Gained every round from the second, times the units <see cref="BaselineGainMultiplier" /> counts.
/// </param>
/// <param name="AddsMoraleLevelResolve">
///     Whether the company's morale level adds its resolve per turn (see <see cref="MoraleLevel.ResolvePerTurn" />).
/// </param>
/// <param name="AddsUnitResolveBonuses">
///     Whether the units' resolve bonuses add to the gain, the <c>MoraleBonusGain</c> statistic that cockpit mods
///     change.
/// </param>
/// <param name="OnlyBiggestUnitResolveBonus">Whether only the biggest unit bonus counts instead of their sum.</param>
/// <param name="InitiativeDelayEffects">
///     What a Precision Strike or a knockdown does to the target, on top of the stability damage.
/// </param>
/// <param name="CanAiGainResolve">Whether the enemy gains resolve at all.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ResolveRules(
    int Min,
    int Max,
    int StartsAt,
    int InspiredAt,
    int InspireCost,
    bool IsAutoInspired,
    IReadOnlyList<StatisticChange> InspiredEffects,
    int BaselineGainPerRound,
    ResolveGainMultiplier BaselineGainMultiplier,
    bool AddsMoraleLevelResolve,
    bool AddsUnitResolveBonuses,
    bool OnlyBiggestUnitResolveBonus,
    ResolveEventThresholds EventThresholds,
    ResolveFromEvents FromEvents,
    IReadOnlyList<StatisticChange> InitiativeDelayEffects,
    bool CanAiGainResolve);

/// <summary>What the baseline gain is multiplied by.</summary>
internal enum ResolveGainMultiplier
{
    None,
    LivingUnits,
    LivingElitePilots
}

/// <param name="MajorityHit">The share of an attack's shots that have to hit for the majority-hit event.</param>
/// <param name="MajorityMiss">The share of shots below which the majority-miss event happens.</param>
/// <param name="MinorArmorDamage">The share of the target's starting armor an attack has to take off.</param>
/// <param name="MajorArmorDamage">The share above which the damage counts as major instead.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ResolveEventThresholds(
    float MajorityHit,
    float MajorityMiss,
    float MinorArmorDamage,
    float MajorArmorDamage);

/// <summary>The resolve the lance gains from what its attacks do to enemies, each once per attack.</summary>
/// <param name="EnemyDestroyed">By the enemy's weight class; a turret counts as light.</param>
/// <param name="EnemyDestroyedInMelee">Added to <see cref="EnemyDestroyed" /> for a melee kill.</param>
/// <param name="ObjectiveCompleted">For a secondary objective; primary ones count nothing.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ResolveFromEvents(
    ByWeightClass<int> EnemyDestroyed,
    int EnemyDestroyedInMelee,
    int EnemyCriticalHit,
    int EnemyAmmoExplosion,
    int EnemyWeaponDestroyed,
    int EnemyLocationDestroyed,
    int EnemyKnockedDown,
    int DeathFromAboveDealt,
    int EnemyMinorArmorDamage,
    int EnemyMajorArmorDamage,
    int MajorityHit,
    int MajorityMiss,
    int ObjectiveCompleted,
    int ObjectiveFailed);
