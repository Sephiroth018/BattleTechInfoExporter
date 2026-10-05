using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A chassis: the frame its mechs share, without anything a loadout decides.</summary>
/// <param name="Name">The name with the variant, as in the references to the chassis.</param>
/// <param name="InitialTonnage">The bare chassis' tonnage, which a mech's tonnage starts from.</param>
/// <param name="HeatDissipation">The heat the chassis itself removes per round, on top of its heat sinks.</param>
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
    IReadOnlyList<ChassisLocationDefinition> Locations);

/// <summary>The distances in meters; a chassis' speed doesn't depend on its loadout.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ChassisMovement(float Walk, float Sprint);

/// <summary>The chassis' melee values, before upgrades add to them.</summary>
/// <param name="DeathFromAboveDamage">The damage of a jump attack onto a target.</param>
/// <param name="DeathFromAboveSelfDamage">The damage the mech takes from its own jump attack.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ChassisMelee(
    float Damage,
    float StabilityDamage,
    float DeathFromAboveDamage,
    float DeathFromAboveSelfDamage);

/// <param name="MaxArmor">The most armor the location can take.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ChassisLocationDefinition(
    ChassisLocations Location,
    LocationArmor MaxArmor,
    float Structure,
    Hardpoints Hardpoints);

/// <param name="Rear"><c>null</c> outside the torso, which alone has rear armor.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LocationArmor(float Front, float? Rear);
