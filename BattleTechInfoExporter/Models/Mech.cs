using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A mech in the mech bay, with its current loadout; each <see cref="Status" /> has a record with the fields of
///     its own.
/// </summary>
/// <param name="Id">The mech's own id, unique among the company's mechs.</param>
/// <param name="Name">The mech's name, which the player can change in the mech lab.</param>
/// <param name="Chassis">The mech's chassis, described in <see cref="Catalog.ChassisDefinitions" />.</param>
/// <param name="Bay">The mech bay row, counted from 1, as the mech bay shows it.</param>
/// <param name="Position">The position in the bay row, counted from 1.</param>
/// <param name="Status">
///     Whether the mech is free for use or held by the mech lab, which decides the mech's other fields.
/// </param>
/// <param name="IsFieldable">Whether the mech can be taken on a mission, as the lance configuration decides.</param>
/// <param name="Problems">The loadout problems the mech lab warns about, e.g. missing ammo, in the game's words.</param>
/// <param name="Loadout">The current loadout, as the mech bay shows it.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record Mech(
    string Id,
    string Name,
    DefinitionReference Chassis,
    int Bay,
    int Position,
    MechStatus Status,
    bool IsFieldable,
    IReadOnlyList<string> Problems,
    MechLoadout Loadout);

/// <summary>A mech free for use.</summary>
/// <param name="Repair">
///     What repairing the mech's damage in the mech bay would take; <c>null</c> without damage.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ReadyMech(
    string Id,
    string Name,
    DefinitionReference Chassis,
    int Bay,
    int Position,
    bool IsFieldable,
    IReadOnlyList<string> Problems,
    MechLoadout Loadout,
    RepairEstimate? Repair)
    : Mech(Id, Name, Chassis, Bay, Position, MechStatus.Ready, IsFieldable, Problems, Loadout);

/// <summary>A mech the mech lab holds for a work order.</summary>
/// <param name="ReadyOnDay">The day the mech lab finishes the work order, as in <see cref="GameState.WorkQueue" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record MechInMechLab(
    string Id,
    string Name,
    DefinitionReference Chassis,
    int Bay,
    int Position,
    MechStatus Status,
    bool IsFieldable,
    IReadOnlyList<string> Problems,
    MechLoadout Loadout,
    int ReadyOnDay)
    : Mech(Id, Name, Chassis, Bay, Position, Status, IsFieldable, Problems, Loadout);

/// <summary>A mech being readied from a stored chassis.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ReadyingMech(
    string Id,
    string Name,
    DefinitionReference Chassis,
    int Bay,
    int Position,
    bool IsFieldable,
    IReadOnlyList<string> Problems,
    MechLoadout Loadout,
    int ReadyOnDay)
    : MechInMechLab(Id, Name, Chassis, Bay, Position, MechStatus.Readying, IsFieldable, Problems, Loadout, ReadyOnDay);

/// <summary>A mech being repaired or refitted; the changes still queued are in <see cref="Refit" />.</summary>
/// <param name="Refit">
///     The steps of the mech's work order, in order; finished steps are already part of the loadout.
/// </param>
/// <param name="AfterRefit">
///     The loadout once every step of <see cref="Refit" /> is finished, as the mech lab shows it.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechInMaintenance(
    string Id,
    string Name,
    DefinitionReference Chassis,
    int Bay,
    int Position,
    bool IsFieldable,
    IReadOnlyList<string> Problems,
    MechLoadout Loadout,
    int ReadyOnDay,
    IReadOnlyList<RefitChange> Refit,
    MechLoadout AfterRefit)
    : MechInMechLab(
        Id,
        Name,
        Chassis,
        Bay,
        Position,
        MechStatus.InMaintenance,
        IsFieldable,
        Problems,
        Loadout,
        ReadyOnDay);

/// <summary>
///     A mech's loadout with the tonnage, value and stats that follow from it; the chassis' limits are in the
///     catalog's <see cref="ChassisDefinition" />.
/// </summary>
/// <param name="UsedTonnage">The tonnage of the chassis, armor and components.</param>
/// <param name="Value">The C-Bill value the game computes from the chassis, armor and components.</param>
/// <param name="Stats">The mech lab's stat bars and the numbers behind them, for this loadout.</param>
/// <param name="Locations">The body locations with their armor, structure and components, from head to legs.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechLoadout(
    float UsedTonnage,
    int Value,
    MechStats Stats,
    IReadOnlyList<MechLocation> Locations);

/// <summary>A mech location's armor and structure; the chassis' limits are in <see cref="ChassisDefinition" />.</summary>
/// <param name="Location">The mech's body location.</param>
/// <param name="Armor">The armor left and fitted, by side.</param>
/// <param name="Structure">The structure left: below the chassis' when damaged, zero when destroyed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record MechLocationCondition(
    ChassisLocations Location,
    LocationArmor<Armor> Armor,
    float Structure);

/// <param name="Components">The components mounted in the location, including the chassis' fixed ones.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechLocation(
    ChassisLocations Location,
    LocationArmor<Armor> Armor,
    float Structure,
    IReadOnlyList<MountedComponent> Components) : MechLocationCondition(Location, Armor, Structure);

/// <param name="Current">
///     What is left after combat damage; refilled to <see cref="Assigned" /> after a mission unless the
///     location was destroyed.
/// </param>
/// <param name="Assigned">The armor fitted in the mech lab.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal readonly record struct Armor(float Current, float Assigned);
