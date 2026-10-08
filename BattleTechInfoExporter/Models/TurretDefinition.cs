using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A turret with its chassis' data: a fixed emplacement with a single location.</summary>
/// <param name="Name">The name the combat HUD shows.</param>
/// <param name="FiringArc">The arc it can fire into, in degrees.</param>
/// <param name="Armor">The armor in combat, after the game's vehicle multiplier.</param>
/// <param name="Structure">The internal structure in combat, after the game's vehicle multiplier.</param>
/// <param name="Components">The components mounted, in the game's order.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record TurretDefinition(
    string Name,
    WeightClass WeightClass,
    float Tonnage,
    float FiringArc,
    float Armor,
    float Structure,
    IReadOnlyList<ComponentReference> Components) : UnitDefinition(Name, WeightClass, Tonnage);
