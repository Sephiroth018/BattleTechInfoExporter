using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A mech: a loadout on a chassis, as a mech assembled from parts or bought comes with it. The frame is in
///     <see cref="Catalog.ChassisDefinitions" />.
/// </summary>
/// <param name="Name">The name with the variant, as in the references to the mech.</param>
/// <param name="Value">The C-Bill value the game computes from the chassis, armor and components.</param>
/// <param name="UsedTonnage">The tonnage of the chassis, armor and components.</param>
/// <param name="IsSalvageable">
///     Whether defeating it can give its mech parts as salvage; the game's <c>BLACKLISTED</c> tag rules it out,
///     e.g. for hero variants.
/// </param>
/// <param name="Locations">The body locations, from head to legs.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechDefinition(
    string Name,
    DefinitionReference Chassis,
    int Value,
    float UsedTonnage,
    MechStats Stats,
    bool IsSalvageable,
    IReadOnlyList<MechLocationDefinition> Locations);

/// <param name="Armor">The armor fitted.</param>
/// <param name="Components">The components mounted in the location, in the game's order.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechLocationDefinition(
    ChassisLocations Location,
    LocationArmor Armor,
    IReadOnlyList<LoadoutComponent> Components);

/// <param name="IsFixed">Part of the chassis; every mech of the chassis has it, and it can't be removed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LoadoutComponent(ComponentReference Component, bool IsFixed);
