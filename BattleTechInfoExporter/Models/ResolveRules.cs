using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The lance's resolve in combat, which Precision Strike and Vigilance spend (see <see cref="SpiritsLevelCosts" />).
/// </summary>
/// <param name="Min">The lowest resolve the lance can have.</param>
/// <param name="Max">The highest resolve the lance can have.</param>
/// <param name="StartsAt">The resolve at the start of a mission, unless the contract sets its own.</param>
/// <param name="InspiredAt">At this resolve the lance's mechs get <see cref="InspiredEffects" />.</param>
/// <param name="InspireCost">The resolve each mech spends when inspired.</param>
/// <param name="IsAutoInspired">
///     Whether the whole lance is inspired as soon as the resolve allows, and loses it below.
/// </param>
/// <param name="InspiredEffects">The statistic changes an inspired mech gets.</param>
/// <param name="BaselineGainPerRound">
///     Gained every round from the second, times the units <see cref="BaselineGainMultiplier" /> counts.
/// </param>
/// <param name="BaselineGainMultiplier">What <see cref="BaselineGainPerRound" /> is multiplied by.</param>
/// <param name="AddsMoraleLevelResolve">
///     Whether the company's morale level adds its resolve per turn (see <see cref="MoraleLevel.ResolvePerTurn" />).
/// </param>
/// <param name="AddsUnitResolveBonuses">
///     Whether the units' resolve bonuses add to the gain, the <c>MoraleBonusGain</c> statistic that cockpit mods
///     change.
/// </param>
/// <param name="OnlyBiggestUnitResolveBonus">Whether only the biggest unit bonus counts instead of their sum.</param>
/// <param name="EventThresholds">The shares of shots and armor at which an attack's events happen.</param>
/// <param name="FromEvents">The resolve gained from what the lance's attacks do and from objectives.</param>
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

/// <summary>
///     The resolve the lance gains from what its attacks do to enemies, each once per attack except kills and major
///     armor damage, which count per enemy, and from failed objectives.
/// </summary>
/// <param name="EnemyDestroyed">By the enemy's weight class; a turret counts as light.</param>
/// <param name="EnemyDestroyedInMelee">Added to <see cref="EnemyDestroyed" /> for a melee kill.</param>
/// <param name="EnemyCriticalHit">For a critical hit on an enemy.</param>
/// <param name="EnemyAmmoExplosion">For an exploding ammunition box or other component of an enemy.</param>
/// <param name="EnemyWeaponDestroyed">For a destroyed weapon of an enemy.</param>
/// <param name="EnemyLocationDestroyed">For a destroyed location of an enemy.</param>
/// <param name="EnemyKnockedDown">For knocking an enemy down.</param>
/// <param name="DeathFromAboveDealt">For a death from above attack that damages its target.</param>
/// <param name="EnemyMinorArmorDamage">
///     For taking off more than <see cref="ResolveEventThresholds.MinorArmorDamage" /> of an enemy's armor.
/// </param>
/// <param name="EnemyMajorArmorDamage">
///     For taking off more than <see cref="ResolveEventThresholds.MajorArmorDamage" /> of an enemy's armor, instead
///     of <see cref="EnemyMinorArmorDamage" />; counted for every enemy the attack damages so.
/// </param>
/// <param name="MajorityHit">
///     For an attack of which more than <see cref="ResolveEventThresholds.MajorityHit" /> of the shots hit.
/// </param>
/// <param name="MajorityMiss">
///     For an attack of which less than <see cref="ResolveEventThresholds.MajorityMiss" /> of the shots hit, when
///     the attack changes the lance's resolve in no other way.
/// </param>
/// <param name="ObjectiveCompleted">For a secondary objective; primary ones count nothing.</param>
/// <param name="ObjectiveFailed">For a failed secondary objective; primary ones count nothing.</param>
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
