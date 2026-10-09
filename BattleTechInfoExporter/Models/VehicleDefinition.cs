using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A vehicle with its chassis' data: its armor and loadout are fixed, as it fights in combat.</summary>
/// <param name="Name">The name the combat HUD shows.</param>
/// <param name="MovementType">
///     How the vehicle moves, <c>Tracked</c> or <c>Wheeled</c>, which picks its row of <see cref="TerrainMoveCosts" />.
/// </param>
/// <param name="Movement">The walk and sprint distances and the pathing.</param>
/// <param name="Locations">The locations from front to rear, then the turret if the vehicle has one.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record VehicleDefinition(
    string Name,
    WeightClass WeightClass,
    float Tonnage,
    VehicleMovementType MovementType,
    ChassisMovement Movement,
    IReadOnlyList<VehicleLocationDefinition> Locations) : UnitDefinition(Name, WeightClass, Tonnage);

/// <param name="Location">The location, e.g. <c>Front</c> or <c>Turret</c>.</param>
/// <param name="Armor">The armor in combat, after the game's vehicle multiplier.</param>
/// <param name="Structure">The internal structure in combat, after the game's vehicle multiplier.</param>
/// <param name="Components">The components mounted in the location, in the game's order.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record VehicleLocationDefinition(
    VehicleChassisLocations Location,
    float Armor,
    float Structure,
    IReadOnlyList<ComponentReference> Components);
