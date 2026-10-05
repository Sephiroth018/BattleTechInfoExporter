using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A mech in the mech bay, with its current loadout; the changes still queued in the mech lab are in
///     <see cref="Refit" />.
/// </summary>
/// <param name="Id">The mech's own id, unique among the company's mechs.</param>
/// <param name="Name">The mech's name, which the player can change in the mech lab.</param>
/// <param name="Bay">The mech bay row, counted from 1, as the mech bay shows it.</param>
/// <param name="Position">The position in the bay row, counted from 1.</param>
/// <param name="Role">The chassis' stock role, e.g. "Brawler".</param>
/// <param name="ReadyOnDay">
///     The day the mech lab finishes its work order, as in <see cref="GameState.WorkQueue" />; <c>null</c> when
///     <see cref="MechStatus.Ready" />.
/// </param>
/// <param name="Refit">
///     The steps of the mech's mech lab work order, in order; finished steps are already part of the loadout.
///     <c>null</c> without a work order and while readying.
/// </param>
/// <param name="IsFieldable">Whether the mech can be taken on a mission, as the lance configuration decides.</param>
/// <param name="Problems">The loadout problems the mech lab warns about, e.g. missing ammo, in the game's words.</param>
/// <param name="Loadout">The current loadout, as the mech bay shows it.</param>
/// <param name="AfterRefit">
///     The loadout once every step of <see cref="Refit" /> is finished, as the mech lab shows it. <c>null</c>
///     without a work order and while readying.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Mech(
    string Id,
    string Name,
    DefinitionReference Chassis,
    WeightClass WeightClass,
    string Role,
    int Bay,
    int Position,
    MechStatus Status,
    int? ReadyOnDay,
    IReadOnlyList<RefitChange>? Refit,
    bool IsFieldable,
    IReadOnlyList<string> Problems,
    MechLoadout Loadout,
    MechLoadout? AfterRefit);

/// <summary>A mech's loadout with the tonnage, value and stats that follow from it.</summary>
/// <param name="Value">The C-Bill value the game computes from the chassis, armor and components.</param>
/// <param name="Locations">The body locations with their armor, structure and components, from head to legs.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechLoadout(
    Tonnage Tonnage,
    int Value,
    MechStats Stats,
    IReadOnlyList<MechLocation> Locations);

/// <param name="Used">The tonnage of the chassis, armor and components.</param>
/// <param name="Max">The chassis' tonnage.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Tonnage(float Used, float Max);

/// <param name="RearArmor"><c>null</c> outside the torso, which alone has rear armor.</param>
/// <param name="Slots">The component slots, used by <see cref="Components" /> and available.</param>
/// <param name="Components">The components mounted in the location, including the chassis' fixed ones.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechLocation(
    ChassisLocations Location,
    Armor Armor,
    Armor? RearArmor,
    Structure Structure,
    Hardpoints Hardpoints,
    Slots Slots,
    IReadOnlyList<MountedComponent> Components);

/// <param name="Current">
///     What is left after combat damage; refilled to <see cref="Assigned" /> after a mission unless the
///     location was destroyed.
/// </param>
/// <param name="Assigned">The armor fitted in the mech lab.</param>
/// <param name="Max">The most armor the location can take.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Armor(float Current, float Assigned, float Max);

/// <param name="Current">Below <see cref="Max" /> when damaged, zero when the location is destroyed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Structure(float Current, float Max);

/// <summary>The most armor a chassis' location can take.</summary>
/// <param name="Rear"><c>null</c> outside the torso, which alone has rear armor.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LocationMaxArmor(ChassisLocations Location, float Front, float? Rear);

/// <summary>The number of weapon hardpoints of each kind.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Hardpoints(int Ballistic, int Energy, int Missile, int Support);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Slots(int Used, int Max);
