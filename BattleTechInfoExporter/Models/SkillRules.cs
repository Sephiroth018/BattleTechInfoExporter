using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The training table of each pilot skill and the limits on choosing primary abilities.</summary>
/// <param name="MaxPrimaryAbilities">How many primary abilities a pilot can hold in total.</param>
/// <param name="MaxPrimaryAbilitiesPerSkill">How many of them can come from one skill.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SkillRules(
    int MaxPrimaryAbilities,
    int MaxPrimaryAbilitiesPerSkill,
    Skill Gunnery,
    Skill Piloting,
    Skill Guts,
    Skill Tactics);

/// <param name="Description">What the skill controls, as the barracks explains it.</param>
/// <param name="Levels">The levels a pilot can train to; every pilot starts at level 1.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Skill(string Description, IReadOnlyList<SkillLevel> Levels);

/// <param name="ExperienceCost">The experience spent to reach the level from the one below.</param>
/// <param name="BaseHitChancePercent">
///     Gunnery only: the hit chance of ranged attacks at this level, before the attack's modifiers.
/// </param>
/// <param name="BaseMeleeHitChancePercent">
///     Piloting only: the hit chance of melee attacks at this level, before the attack's modifiers.
/// </param>
/// <param name="Abilities">The primary abilities and passive traits unlocked at the level.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SkillLevel(
    int Level,
    int ExperienceCost,
    float? BaseHitChancePercent,
    float? BaseMeleeHitChancePercent,
    IReadOnlyList<SkillLevelAbility> Abilities);

/// <param name="IsPrimary">A primary ability is chosen, a passive trait comes with the level.</param>
/// <param name="Targeting">How the ability is aimed; <c>null</c> for one that isn't.</param>
/// <param name="Cooldown">The rounds before the ability can be used again; <c>null</c> without a cooldown.</param>
/// <param name="Uses">How often the ability can be used per mission; <c>null</c> without a limit.</param>
/// <param name="Effects">
///     The statistics the ability changes; empty for an ability whose effect the game hardcodes, e.g.
///     Multi-Target or Sensor Lock.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SkillLevelAbility(
    DefinitionReference Ability,
    bool IsPrimary,
    string Description,
    AbilityDef.ActivationTiming ActivationTiming,
    AbilityDef.TargetingType? Targeting,
    int? Cooldown,
    int? Uses,
    IReadOnlyList<StatisticChange> Effects);
