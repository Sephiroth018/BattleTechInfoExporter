using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The fields every unit definition in the catalog starts with; the chassis, vehicle and turret definitions derive
///     from it.
/// </summary>
/// <param name="Name">As in the references to the unit.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record UnitDefinition(string Name, WeightClass WeightClass, float Tonnage);
