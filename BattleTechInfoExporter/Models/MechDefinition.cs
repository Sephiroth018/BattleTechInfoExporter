using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A mech: a loadout on a chassis, as a mech assembled from parts or bought comes with it. The frame is in
///     <see cref="Catalog.ChassisDefinitions" />. The game's appearance dates aren't exported, as the campaign
///     ignores them; nor are the company tags a few DLC mechs need before enemy lances field them, which finishing a
///     flashpoint adds.
/// </summary>
/// <param name="Name">The name with the variant, as in the references to the mech.</param>
/// <param name="Chassis">The mech's chassis, in <see cref="Catalog.ChassisDefinitions" />.</param>
/// <param name="Role">
///     The chassis' stock role, e.g. "Brawler", as the mech tooltip and the mech lab show it: what its standard
///     fitting is built for, which a refit can change; no rule reads it.
/// </param>
/// <param name="Description">
///     The mech's description, as the mech tooltip and the store show it, or the chassis' one the mech bay shows
///     where the mech has none, as plain text: it describes the standard fitting, which a refit can change; no rule
///     reads it.
/// </param>
/// <param name="Value">The C-Bill value the game computes from the chassis, armor and components.</param>
/// <param name="UsedTonnage">The tonnage of the chassis, armor and components.</param>
/// <param name="Stats">The performance summary of the mech with this loadout.</param>
/// <param name="IsSalvageable">
///     Whether defeating it can give its mech parts as salvage; the game's <c>BLACKLISTED</c> tag rules it out,
///     e.g. for hero variants.
/// </param>
/// <param name="Locations">The body locations, from head to legs.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechDefinition(
    string Id,
    string Name,
    DefinitionReference Chassis,
    string Role,
    string Description,
    int Value,
    float UsedTonnage,
    MechStats Stats,
    bool IsSalvageable,
    IReadOnlyList<MechLocationDefinition> Locations) : ICatalogEntry;

/// <summary>A body location of a mech definition, with its armor and components.</summary>
/// <param name="Location">The body location, e.g. <c>LeftTorso</c>.</param>
/// <param name="Armor">The armor fitted.</param>
/// <param name="Components">The components mounted in the location, in the game's order.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechLocationDefinition(
    ChassisLocations Location,
    LocationArmor<float> Armor,
    IReadOnlyList<LoadoutComponent> Components);

/// <summary>A component of a mech definition's loadout.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LoadoutComponent(ComponentReference Component, bool IsFixed)
    : ComponentEntry(Component), IFixedOrRemovable;
