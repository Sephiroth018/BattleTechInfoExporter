using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A chassis: the frame its mechs share, without anything a loadout decides.</summary>
/// <param name="Name">The name with the variant, as in the references to the chassis.</param>
/// <param name="InitialTonnage">The bare chassis' tonnage, which a mech's tonnage starts from.</param>
/// <param name="MaxJumpJets">The most jump jets a mech of the chassis can mount; 0 when it can't jump.</param>
/// <param name="HeatDissipation">The heat the chassis itself removes per round, on top of its heat sinks.</param>
/// <param name="Movement">The walk and sprint distances and the pathing, which no loadout changes.</param>
/// <param name="Melee">The damage of melee and jump attacks, before upgrades add to them.</param>
/// <param name="StockMech">
///     The mech the game treats as the chassis' stock loadout; <c>null</c> when its definition is missing.
/// </param>
/// <param name="Locations">The body locations, from head to legs.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ChassisDefinition(
    string Name,
    WeightClass WeightClass,
    float Tonnage,
    float InitialTonnage,
    int MaxJumpJets,
    int HeatDissipation,
    ChassisMovement Movement,
    ChassisMelee Melee,
    DefinitionReference? StockMech,
    IReadOnlyList<ChassisLocationDefinition> Locations) : UnitDefinition(Name, WeightClass, Tonnage);

/// <summary>How the unit moves, with the distances in meters; a chassis' speed doesn't depend on its loadout.</summary>
/// <param name="Walk">
///     The movement of a walk, which each step uses up per meter by its terrain's move cost
///     (<see cref="TerrainDefinition.MoveCost" />).
/// </param>
/// <param name="Sprint">
///     The movement of a sprint, which each step uses up per meter by its terrain's move cost
///     (<see cref="TerrainDefinition.MoveCost" />) times its <see cref="TerrainDefinition.SprintMultiplier" />.
/// </param>
/// <param name="PathingId">
///     The game's pathing capabilities the unit moves with, which decide the slopes it can climb, as in
///     <c>combat-map.json</c>'s <see cref="CombatMap.PathingGroups" />.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ChassisMovement(float Walk, float Sprint, string PathingId);

/// <summary>The chassis' melee values, before upgrades add to them.</summary>
/// <param name="Damage">The damage of a melee attack.</param>
/// <param name="StabilityDamage">The stability damage of a melee attack.</param>
/// <param name="DeathFromAboveDamage">The damage of a jump attack onto a target.</param>
/// <param name="DeathFromAboveSelfDamage">The damage the mech takes from its own jump attack.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ChassisMelee(
    float Damage,
    float StabilityDamage,
    float DeathFromAboveDamage,
    float DeathFromAboveSelfDamage);

/// <param name="Location">The body location, e.g. <c>LeftTorso</c>.</param>
/// <param name="MaxArmor">The most armor the location can take.</param>
/// <param name="Structure">The location's internal structure.</param>
/// <param name="Hardpoints">The location's weapon hardpoints.</param>
/// <param name="Slots">The component slots, which a loadout's components fill by their own slots.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ChassisLocationDefinition(
    ChassisLocations Location,
    LocationArmor MaxArmor,
    float Structure,
    Hardpoints Hardpoints,
    int Slots);

/// <param name="Front">The front armor; outside the torso, the location's only armor.</param>
/// <param name="Rear"><c>null</c> outside the torso, which alone has rear armor.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LocationArmor(float Front, float? Rear);

/// <summary>The number of weapon hardpoints of each kind.</summary>
/// <param name="Ballistic">For weapons of the <c>Ballistic</c> <see cref="WeaponDefinition.Category" />.</param>
/// <param name="Energy">For weapons of the <c>Energy</c> <see cref="WeaponDefinition.Category" />.</param>
/// <param name="Missile">For weapons of the <c>Missile</c> <see cref="WeaponDefinition.Category" />.</param>
/// <param name="Support">For weapons of the <c>Support</c> <see cref="WeaponDefinition.Category" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Hardpoints(int Ballistic, int Energy, int Missile, int Support);
