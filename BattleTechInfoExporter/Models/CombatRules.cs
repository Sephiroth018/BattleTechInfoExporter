using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The combat rules: the constants behind to-hit, evasion, cover, heat, stability, melee, hit locations,
///     visibility, resolve and movement. A to-hit modifier is added to the attack's difficulty: positive makes the
///     shot harder, negative is a bonus.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatRules(
    ToHitRules ToHit,
    EvasionRules Evasion,
    GuardRules Guard,
    LineOfFireRules LineOfFire,
    HeatRules Heat,
    StabilityRules Stability,
    InjuryRules Injuries,
    MeleeAttackRules Melee,
    CriticalHitRules CriticalHits,
    HitTables HitTables,
    VisibilityRules Visibility,
    ResolveRules Resolve,
    MovementRules Movement);
