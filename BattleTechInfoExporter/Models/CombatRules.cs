using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The combat rules: the constants behind to-hit, evasion, cover, heat, stability, melee, hit locations,
///     visibility, resolve and movement. A to-hit modifier is added to the attack's difficulty: positive makes the
///     shot harder, negative is a bonus.
/// </summary>
/// <param name="ToHit">How an attack's hit chance follows from its modifiers.</param>
/// <param name="Evasion">The evasive pips units earn by moving.</param>
/// <param name="Guard">How guard levels reduce damage.</param>
/// <param name="LineOfFire">How cover between attacker and target affects ranged attacks.</param>
/// <param name="Heat">The heat rules of mechs.</param>
/// <param name="Stability">The stability rules of mechs.</param>
/// <param name="Injuries">What injures a mech's pilot.</param>
/// <param name="Melee">The melee rules, jump attacks included.</param>
/// <param name="CriticalHits">The chance of critical hits on a mech's components.</param>
/// <param name="HitTables">Which location a hit lands on.</param>
/// <param name="Visibility">Spotting, sensors and sensor locks.</param>
/// <param name="Resolve">The lance's resolve in combat.</param>
/// <param name="Movement">What shortens a mech's movement.</param>
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
