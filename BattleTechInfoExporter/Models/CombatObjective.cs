using System.Collections.Generic;
using BattleTech.Framework;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>An objective in the HUD's objective list.</summary>
/// <param name="Id">The game's id of the objective, which <see cref="ObjectiveZone.ObjectiveIds" /> refer to.</param>
/// <param name="IsPrimary">Whether the HUD lists it as a primary objective.</param>
/// <param name="Progress">
///     The progress line the HUD shows under the title, e.g. a count of units destroyed; <c>null</c>
///     where it shows none.
/// </param>
/// <param name="TargetUnitIds">
///     The units it is about (to destroy, protect or escort), as keys of <see cref="CombatState.Units" />, while
///     they are in full view: neither blips nor destroyed enemies. Buildings are left out.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatObjective(
    string Id,
    string Title,
    ObjectiveStatus Status,
    bool IsPrimary,
    string? Progress,
    IReadOnlyList<string> TargetUnitIds);

/// <summary>A zone drawn on the map, e.g. a capture or evacuation zone.</summary>
/// <param name="Type">The game's id of the zone type, e.g. <c>regionDef_EvacZone</c>.</param>
/// <param name="Radius">
///     The radius of the hexagon the HUD draws; whether a unit is inside is decided per map cell, which this only
///     approximates.
/// </param>
/// <param name="IsPreview">Drawn as a zone that becomes active later.</param>
/// <param name="ObjectiveIds">The objectives the zone belongs to, as <see cref="CombatObjective.Id" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ObjectiveZone(
    string Id,
    string Type,
    MapPosition Center,
    float Radius,
    bool IsPreview,
    IReadOnlyList<string> ObjectiveIds);
