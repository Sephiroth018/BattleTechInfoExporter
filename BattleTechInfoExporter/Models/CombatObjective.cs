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
///     when it shows none.
/// </param>
/// <param name="TargetUnitIds">
///     The units it is about (to destroy, protect or escort), as keys of <see cref="CombatState.Units" />, while
///     they are in full view, destroyed ones included; blips are left out.
/// </param>
/// <param name="TargetBuildingIds">
///     The buildings it is about (to destroy or defend), by their <see cref="CombatBuilding.Id" />.
/// </param>
/// <param name="Beacon">
///     The hex the HUD draws the objective's beacon above, e.g. where the reinforcements it announces will land;
///     <c>null</c> when it draws none: an objective that marks its target units instead, or a finished one.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatObjective(
    string Id,
    string Title,
    ObjectiveStatus Status,
    bool IsPrimary,
    string? Progress,
    IReadOnlyList<string> TargetUnitIds,
    IReadOnlyList<string> TargetBuildingIds,
    HexCoordinates? Beacon);

/// <summary>A zone drawn on the map, e.g. a capture or evacuation zone.</summary>
/// <param name="Id">The game's id of the zone.</param>
/// <param name="Type">
///     The game's id of the zone's region definition, e.g. <c>regionDef_EvacZone</c>, which names what the zone is
///     for.
/// </param>
/// <param name="Hexes">
///     The playable hexes whose center cell is in the zone, by <c>r</c>, then <c>q</c>, as in
///     <see cref="CombatMap.Hexes" />: a unit standing on one is in it. The game decides per map cell, so a unit
///     whose footprint reaches into the zone from a hex next to these can count as in it too.
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
    IReadOnlyList<HexCoordinates> Hexes,
    bool IsPreview,
    IReadOnlyList<string> ObjectiveIds);
