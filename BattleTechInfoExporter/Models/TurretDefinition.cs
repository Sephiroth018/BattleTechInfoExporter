using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A turret with its chassis' data: a fixed emplacement with a single location.</summary>
/// <param name="Name">The name the combat HUD shows.</param>
/// <param name="FiringArcDegrees">The arc it can fire into without turning, in degrees.</param>
/// <param name="Armor">The armor in combat, after the game's vehicle multiplier; a turret has no rear armor.</param>
/// <param name="Structure">The internal structure in combat, after the game's vehicle multiplier.</param>
/// <param name="Components">The components mounted, in the game's order, all fixed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record TurretDefinition(
    string Id,
    string Name,
    WeightClass WeightClass,
    float Tonnage,
    float FiringArcDegrees,
    LocationArmor<float> Armor,
    float Structure,
    IReadOnlyList<LoadoutComponent> Components) : UnitDefinition(Id, Name, WeightClass, Tonnage);
