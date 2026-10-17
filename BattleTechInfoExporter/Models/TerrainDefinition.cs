using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A terrain's effects on the units in it: forest, water, rough ground, roads and the like. A cell has one
///     terrain at most, so the effects never stack; the map's biome applies on top (see
///     <see cref="BiomeDefinition" />). Only the effects the game's combat code reads are exported; the tooltips also
///     name a stability damage multiplier, which only the AI reads.
/// </summary>
/// <param name="Name">As the combat HUD shows it.</param>
/// <param name="MoveCost">
///     What a step into the terrain costs of the unit's movement, per meter of the step; <c>null</c> when the unit
///     can't enter it. Open ground has no terrain and costs the normal move cost of the unit's pathing capabilities
///     instead, 1.05 for every unit in the game's data.
/// </param>
/// <param name="SprintMultiplier">Multiplies the move cost when sprinting; <c>null</c> when the unit can't enter it.</param>
/// <param name="VisibilityMultiplier">
///     Multiplies the spotting distance a sight line uses up while it passes through the terrain below
///     <paramref name="VisibilityHeight" />.
/// </param>
/// <param name="VisibilityHeight">In meters above the ground.</param>
/// <param name="SensorRangeMultiplier">Multiplies the sensor range of a unit in the terrain.</param>
/// <param name="SignatureMultiplier">Multiplies the signature of a unit in the terrain.</param>
/// <param name="ToHitAgainstRanged">
///     The to-hit modifier of ranged attacks against a mech or vehicle in the terrain, not a turret; positive makes
///     the attack harder, as in <see cref="CombatRules.ToHit" />.
/// </param>
/// <param name="ToHitAgainstMelee">
///     The to-hit modifier of melee attacks against a mech or vehicle in the terrain, not a turret; positive makes
///     the attack harder.
/// </param>
/// <param name="ToHitFromRanged">
///     The to-hit modifier of ranged attacks by a unit in the terrain; positive makes the attack harder.
/// </param>
/// <param name="GrantsGuarded">Whether a unit in the terrain has a guard level from cover.</param>
/// <param name="HeatSinkMultiplier">Multiplies the heat sink capacity of a mech in the terrain.</param>
/// <param name="HeatPerRound">Added to a mech's heat at the end of its activation in the terrain.</param>
/// <param name="DamageDealt">Multiplies the damage of a unit firing from the terrain, by weapon category.</param>
/// <param name="DamageTaken">
///     Multiplies the damage a unit in the terrain takes, by weapon category; buildings take none.
/// </param>
/// <param name="StickyEffects">
///     What a unit gets when it enters, crosses or starts its activation in the terrain.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record TerrainDefinition(
    string Id,
    string Name,
    TerrainMoveCosts MoveCost,
    float? SprintMultiplier,
    float VisibilityMultiplier,
    float VisibilityHeight,
    float SensorRangeMultiplier,
    float SignatureMultiplier,
    float ToHitAgainstRanged,
    float ToHitAgainstMelee,
    float ToHitFromRanged,
    bool GrantsGuarded,
    float HeatSinkMultiplier,
    int HeatPerRound,
    DamageMultipliers DamageDealt,
    DamageMultipliers DamageTaken,
    IReadOnlyList<StatisticChange> StickyEffects) : ICatalogEntry;

/// <summary>By the unit: a mech, a tracked vehicle or a wheeled vehicle, and its weight class.</summary>
/// <param name="Mech">For mechs, by their weight class; <c>null</c> when they can't enter it.</param>
/// <param name="Tracked">For tracked vehicles, by their weight class; <c>null</c> when they can't enter it.</param>
/// <param name="Wheeled">For wheeled vehicles, by their weight class; <c>null</c> when they can't enter it.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record TerrainMoveCosts(
    ByWeightClass<float?> Mech,
    ByWeightClass<float?> Tracked,
    ByWeightClass<float?> Wheeled);

/// <summary>
///     The multiplier for every attack and the weapon category's both apply; melee and jump attacks get only
///     the one for every attack.
/// </summary>
/// <param name="All">For every attack.</param>
/// <param name="ByWeaponCategory">
///     The weapon category's multiplier keyed by every <see cref="WeaponDefinition.Category" /> the game knows, as
///     <see cref="CombatRules.WeaponCategories" /> are; 1 for a category without one, such as <c>Melee</c>.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record DamageMultipliers(float All, IReadOnlyDictionary<string, float> ByWeaponCategory);
