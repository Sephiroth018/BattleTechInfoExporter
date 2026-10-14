using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The combat rules: the constants behind to-hit, evasion, cover, damage, heat, stability, melee, hit
///     locations, visibility, resolve and movement. A to-hit modifier is added to the attack's difficulty:
///     positive makes the shot harder, negative is a bonus.
/// </summary>
/// <param name="ToHit">How an attack's hit chance follows from its modifiers.</param>
/// <param name="Evasion">The evasive pips units earn by moving.</param>
/// <param name="Guard">How guard levels reduce damage.</param>
/// <param name="LineOfFire">How cover between attacker and target affects ranged attacks.</param>
/// <param name="Damage">The damage multipliers that apply regardless of the target.</param>
/// <param name="WeaponCategories">
///     By the weapon category's name, as the catalog's weapons name theirs; <c>Melee</c> is the melee and jump
///     attacks'.
/// </param>
/// <param name="StrayShots">What a missed shot can hit instead; <c>null</c> when misses hit nothing.</param>
/// <param name="Coil">How COIL weapons gain from the attacker's evasive pips.</param>
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
    DamageRules Damage,
    IReadOnlyDictionary<string, WeaponCategoryRules> WeaponCategories,
    StrayShotRules? StrayShots,
    CoilRules Coil,
    HeatRules Heat,
    StabilityRules Stability,
    InjuryRules Injuries,
    MeleeAttackRules Melee,
    CriticalHitRules CriticalHits,
    HitTables HitTables,
    VisibilityRules Visibility,
    ResolveRules Resolve,
    MovementRules Movement);
