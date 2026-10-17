using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The training table of each pilot skill and the limits on choosing primary abilities.</summary>
/// <param name="MaxPrimaryAbilities">How many primary abilities a pilot can hold in total.</param>
/// <param name="MaxPrimaryAbilitiesPerSkill">How many of them can come from one skill.</param>
/// <param name="Gunnery">The Gunnery skill's training table.</param>
/// <param name="Piloting">The Piloting skill's training table.</param>
/// <param name="Guts">The Guts skill's training table.</param>
/// <param name="Tactics">The Tactics skill's training table.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SkillRules(
    int MaxPrimaryAbilities,
    int MaxPrimaryAbilitiesPerSkill,
    Skill<GunneryLevel> Gunnery,
    Skill<PilotingLevel> Piloting,
    Skill<SkillLevel> Guts,
    Skill<SkillLevel> Tactics);

/// <summary>A pilot skill's training table.</summary>
/// <param name="Description">What the skill controls, as the barracks explains it.</param>
/// <param name="Levels">The levels a pilot can train to; every pilot starts at level 1.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Skill<TLevel>(string Description, IReadOnlyList<TLevel> Levels)
    where TLevel : SkillLevel;

/// <summary>A level of a skill; a skill with numbers of its own per level adds them in its shape.</summary>
/// <param name="Level">The skill level, from 2.</param>
/// <param name="ExperienceCost">The experience spent to reach the level from the one below.</param>
/// <param name="Abilities">The primary abilities and passive traits unlocked at the level.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal record SkillLevel(int Level, int ExperienceCost, IReadOnlyList<SkillLevelAbility> Abilities);

/// <summary>A level of the Gunnery skill.</summary>
/// <param name="BaseHitChancePercent">
///     The hit chance of ranged attacks at this level, before the attack's modifiers.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record GunneryLevel(
    int Level,
    int ExperienceCost,
    IReadOnlyList<SkillLevelAbility> Abilities,
    float BaseHitChancePercent) : SkillLevel(Level, ExperienceCost, Abilities);

/// <summary>A level of the Piloting skill.</summary>
/// <param name="BaseMeleeHitChancePercent">
///     The hit chance of melee attacks at this level, before the attack's modifiers.
/// </param>
/// <param name="MovementAfterStandingUpPercent">
///     The share of its walk, sprint and backward distance a mech keeps in the round it stands up.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record PilotingLevel(
    int Level,
    int ExperienceCost,
    IReadOnlyList<SkillLevelAbility> Abilities,
    float BaseMeleeHitChancePercent,
    float MovementAfterStandingUpPercent) : SkillLevel(Level, ExperienceCost, Abilities);

/// <summary>An ability a skill level unlocks.</summary>
/// <param name="Ability">The ability, by its definition's id and name.</param>
/// <param name="IsPrimary">A primary ability is chosen, a passive trait comes with the level.</param>
/// <param name="Description">The game's description of the ability, as plain text.</param>
/// <param name="ActivationTiming">
///     The game's activation timing of the ability, e.g. <c>Passive</c>, <c>ConsumedByFiring</c> or
///     <c>CommandAbility</c>.
/// </param>
/// <param name="Targeting">How the ability is aimed; <c>null</c> when it isn't.</param>
/// <param name="Cooldown">
///     The unit's activations before the ability can be used again; <c>null</c> when it has no cooldown.
/// </param>
/// <param name="Uses">How often the ability can be used per mission; <c>null</c> when there's no limit.</param>
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
