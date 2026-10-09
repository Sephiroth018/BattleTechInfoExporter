using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A terrain's effects on the units in it: forest, water, rough ground, roads and the like. A cell has one
///     terrain at most, so the effects never stack; the map's biome applies on top (see
///     <see cref="BiomeDefinition" />).
/// </summary>
/// <param name="Name">As the combat HUD shows it.</param>
/// <param name="MoveCost">
///     What a step into the terrain costs of the unit's movement, per meter of the step: open ground costs 1.05;
///     <c>null</c> where the unit can't enter.
/// </param>
/// <param name="SprintMultiplier">Multiplies the move cost when sprinting.</param>
/// <param name="VisibilityMultiplier">
///     Multiplies the spotting distance a sight line uses up while it passes through the terrain below
///     <paramref name="VisibilityHeight" />.
/// </param>
/// <param name="VisibilityHeight">In meters above the ground.</param>
/// <param name="SensorRangeMultiplier">Multiplies the sensor range of a unit in the terrain.</param>
/// <param name="SignatureMultiplier">Multiplies the signature of a unit in the terrain.</param>
/// <param name="RangedTargetability">The to-hit modifier of ranged attacks against a unit in the terrain.</param>
/// <param name="MeleeTargetability">The to-hit modifier of melee attacks against a unit in the terrain.</param>
/// <param name="ToHitFrom">The to-hit modifier of ranged attacks by a unit in the terrain.</param>
/// <param name="GrantsGuarded">Whether a unit in the terrain has a guard level from cover.</param>
/// <param name="HeatSinkMultiplier">Multiplies the heat sink capacity of a mech in the terrain.</param>
/// <param name="HeatPerTurn">Added to a mech's heat at the end of its activation in the terrain.</param>
/// <param name="DamageDealt">Multiplies the damage of a unit firing from the terrain, by weapon category.</param>
/// <param name="DamageTaken">Multiplies the damage a unit in the terrain takes, by weapon category.</param>
/// <param name="StickyEffects">
///     What a unit gets when it enters, crosses or starts its activation in the terrain.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record TerrainDefinition(
    string Name,
    TerrainMoveCosts MoveCost,
    float SprintMultiplier,
    float VisibilityMultiplier,
    float VisibilityHeight,
    float SensorRangeMultiplier,
    float SignatureMultiplier,
    float RangedTargetability,
    float MeleeTargetability,
    float ToHitFrom,
    bool GrantsGuarded,
    float HeatSinkMultiplier,
    int HeatPerTurn,
    DamageMultipliers DamageDealt,
    DamageMultipliers DamageTaken,
    IReadOnlyList<StatisticChange> StickyEffects);

/// <summary>By the unit: a mech, a tracked vehicle or a wheeled vehicle, and its weight class.</summary>
/// <param name="Mech">For mechs, by their weight class.</param>
/// <param name="Tracked">For tracked vehicles, by their weight class.</param>
/// <param name="Wheeled">For wheeled vehicles, by their weight class.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record TerrainMoveCosts(
    ByWeightClass<float?> Mech,
    ByWeightClass<float?> Tracked,
    ByWeightClass<float?> Wheeled);

/// <summary>
///     <see cref="All" /> and the weapon category's multiplier both apply; melee has no category.
/// </summary>
/// <param name="All">For every attack.</param>
/// <param name="Support">For weapons of the <c>Support</c> <see cref="WeaponDefinition.Category" />.</param>
/// <param name="Energy">For weapons of the <c>Energy</c> <see cref="WeaponDefinition.Category" />.</param>
/// <param name="Ballistic">For weapons of the <c>Ballistic</c> <see cref="WeaponDefinition.Category" />.</param>
/// <param name="Missile">For weapons of the <c>Missile</c> <see cref="WeaponDefinition.Category" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record DamageMultipliers(float All, float Support, float Energy, float Ballistic, float Missile);
