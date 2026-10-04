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
/// <param name="DaysUntilReady">
///     The days until the mech lab finishes its work order; zero when
///     <see cref="MechStatus.Ready" />.
/// </param>
/// <param name="WorkQueuePosition">
///     The position of the mech's work order in the mech lab queue, counted from 1; the mech techs work on the
///     first one only. <c>null</c> without a work order.
/// </param>
/// <param name="Refit">
///     The steps of the mech's mech lab work order, in order; finished steps are already part of the loadout.
///     <c>null</c> without a work order and while readying.
/// </param>
/// <param name="IsFieldable">Whether the mech can be taken on a mission, as the lance configuration decides.</param>
/// <param name="Problems">The loadout problems the mech lab warns about, e.g. missing ammo, in the game's words.</param>
/// <param name="Value">The C-Bill value the game computes from the chassis, armor and equipment.</param>
/// <param name="Locations">The body locations with their armor, structure and equipment, from head to legs.</param>
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
    int DaysUntilReady,
    int? WorkQueuePosition,
    IReadOnlyList<RefitChange>? Refit,
    bool IsFieldable,
    IReadOnlyList<string> Problems,
    Tonnage Tonnage,
    int Value,
    MechStats Stats,
    IReadOnlyList<MechLocation> Locations);

/// <param name="Used">The tonnage of the chassis, armor and equipment.</param>
/// <param name="Max">The chassis' tonnage.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Tonnage(float Used, float Max);

/// <param name="RearArmor"><c>null</c> outside the torso, which alone has rear armor.</param>
/// <param name="Slots">The equipment slots, used by <see cref="Equipment" /> and available.</param>
/// <param name="Equipment">The equipment mounted in the location, including the chassis' fixed equipment.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechLocation(
    ChassisLocations Location,
    Armor Armor,
    Armor? RearArmor,
    Structure Structure,
    Hardpoints Hardpoints,
    Slots Slots,
    IReadOnlyList<Equipment> Equipment);

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

/// <summary>The number of weapon hardpoints of each kind.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Hardpoints(int Ballistic, int Energy, int Missile, int Support);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Slots(int Used, int Max);
