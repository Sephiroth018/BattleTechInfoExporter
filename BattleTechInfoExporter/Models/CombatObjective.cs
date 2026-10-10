using System.Collections.Generic;
using BattleTech.Framework;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>An objective in the HUD's objective list.</summary>
/// <param name="Id">The game's id of the objective, which <see cref="ObjectiveZone.ObjectiveIds" /> refer to.</param>
/// <param name="Title">The title the HUD shows, as plain text.</param>
/// <param name="Status">The objective's game status, e.g. <c>Active</c>, <c>Succeeded</c> or <c>Failed</c>.</param>
/// <param name="IsPrimary">Whether the HUD lists it as a primary objective.</param>
/// <param name="Progress">
///     The progress line the HUD shows under the title, e.g. a count of units destroyed; <c>null</c>
///     where it shows none.
/// </param>
/// <param name="TargetUnitIds">
///     The units it is about (to destroy, protect or escort), as keys of <see cref="CombatState.Units" />, while
///     they are in full view: neither blips nor destroyed enemies.
/// </param>
/// <param name="TargetBuildingIds">
///     The buildings it is about (to destroy or defend), as <see cref="CombatBuilding.Id" /> in
///     <see cref="CombatMap.Buildings" />.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatObjective(
    string Id,
    string Title,
    ObjectiveStatus Status,
    bool IsPrimary,
    string? Progress,
    IReadOnlyList<string> TargetUnitIds,
    IReadOnlyList<string> TargetBuildingIds);

/// <summary>A zone drawn on the map, e.g. a capture or evacuation zone.</summary>
/// <param name="Id">The game's id of the zone.</param>
/// <param name="Type">The game's id of the zone type, e.g. <c>regionDef_EvacZone</c>.</param>
/// <param name="Center">The zone's center on the map.</param>
/// <param name="Radius">
///     The radius of the hexagon the HUD draws; whether a unit is inside is decided per map cell, which this only
///     approximates.
/// </param>
/// <param name="IsPreview">Drawn as a zone that becomes active later.</param>
/// <param name="ObjectiveIds">
///     The objectives the zone belongs to among <see cref="CombatState.Objectives" />, as
///     <see cref="CombatObjective.Id" />; empty where it belongs only to objectives the HUD doesn't list.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ObjectiveZone(
    string Id,
    string Type,
    MapPosition Center,
    float Radius,
    bool IsPreview,
    IReadOnlyList<string> ObjectiveIds);
