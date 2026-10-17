using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The root of <c>combat-state.json</c>: the battle as the player's HUD shows it, written when every phase
///     begins and after every unit's activation; the file exists only while a battle is running.
/// </summary>
/// <param name="Contract">The contract being fought, as in <see cref="MissionOutcome.Contract" />.</param>
/// <param name="MapId">The battle's map, as <see cref="CombatMap.MapId" />.</param>
/// <param name="Round">The combat round, counted from 1.</param>
/// <param name="Phase">
///     The current phase as the HUD numbers it, as <see cref="CombatUnitState.Initiative" />: units act from 5
///     down to 1.
/// </param>
/// <param name="Resolve">The lance's resolve now; its limits are in <see cref="CombatRules.Resolve" />.</param>
/// <param name="MovementLegend">How to read the units' <see cref="CombatUnit.Movement" />.</param>
/// <param name="Units">
///     Every unit the player knows of, keyed by the game's unit id: the player's and allied units, and the enemies
///     and neutral units the HUD shows or last showed. A destroyed unit stays, as its wreck is on the map; one that
///     withdrew or that the mission's script removed leaves no wreck and is left out.
/// </param>
/// <param name="Objectives">The objectives the HUD lists, finished ones included, in the HUD's order.</param>
/// <param name="Zones">The zones drawn on the map.</param>
/// <param name="LandingZones">
///     Where a dropship or drop pods are about to land; entries come and go as the mission readies landings and
///     they happen. The game ties none to the objective announcing it: match them by position with the
///     objective's <c>beacon</c>. A lance arriving without drop pods or a dropship, e.g. at a zone of type
///     <c>regionDef_HostileDropZone</c>, has none, and the band along the encounter boundary the HUD marks the same
///     way isn't exported.
/// </param>
/// <param name="BuildingSides">
///     The buildings on a side, e.g. a base to destroy or defend, by their <see cref="CombatBuilding.Id" />, in the
///     order of their ids; the others are on none. The mission's script can change a building's side during the
///     battle.
/// </param>
/// <param name="DamagedBuildings">
///     The buildings below their max structure, destroyed ones included, by their <see cref="CombatBuilding.Id" />,
///     in the order of their ids; the others are undamaged.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatState(
    MissionContract Contract,
    string? MapId,
    int Round,
    int Phase,
    int Resolve,
    MovementLegend MovementLegend,
    IReadOnlyDictionary<string, CombatUnit> Units,
    IReadOnlyList<CombatObjective> Objectives,
    IReadOnlyList<ObjectiveZone> Zones,
    IReadOnlyList<LandingZone> LandingZones,
    IReadOnlyList<BuildingSide> BuildingSides,
    IReadOnlyList<DamagedBuilding> DamagedBuildings) : ExportFile
{
    internal const string FileName = "combat-state.json";
}

/// <summary>A landing zone the HUD marks as dangerous.</summary>
/// <param name="Id">The game's id of the dropship, or of the lance spawner whose drop pods land there.</param>
/// <param name="Kind">What is about to land.</param>
/// <param name="Hexes">
///     The hexes where a unit is crushed by the landing, those whose center cell the game marks, in the order of
///     <see cref="CombatMap.Rows" />: a dropship's whole footprint, or one hex per drop pod, one per unit of the
///     lance.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LandingZone(string Id, LandingZoneKind Kind, IReadOnlyList<HexCoordinates> Hexes);

/// <summary>A hex of <see cref="CombatMap.Rows" /> by its axial coordinates.</summary>
/// <param name="Q">The hex's axial <c>q</c>, as in <see cref="CombatMap.HexGrid" />.</param>
/// <param name="R">The hex's axial <c>r</c>, as in <see cref="CombatMap.HexGrid" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HexCoordinates(int Q, int R);

/// <summary>A building on a side, highlighted in its team's color on the map.</summary>
/// <param name="Id">The building's id in <see cref="CombatMap.Buildings" />.</param>
/// <param name="Side">Whose side the building is on.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record BuildingSide(string Id, TeamSide Side);

/// <summary>A building below its max structure, or destroyed.</summary>
/// <param name="Id">The building's id in <see cref="CombatMap.Buildings" />.</param>
/// <param name="Structure">
///     The structure left, which the HUD shows when the building is targeted; its max is in
///     <see cref="CombatBuilding.MaxStructure" />.
/// </param>
/// <param name="DestroyedHexes">
///     The hexes the building stood on as they are now, since <c>combat-map.json</c> isn't rewritten when it falls:
///     units on its roof drop to the ground, which the rubble's terrain covers. <c>null</c> when it stands. The
///     blocked steps onto and off them still follow the roof, and the terrain a building's explosion changes around
///     it isn't exported.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record DamagedBuilding(string Id, float Structure, IReadOnlyList<MapHex>? DestroyedHexes);

/// <summary>A hex of <see cref="CombatMap.Rows" /> as it is now.</summary>
/// <param name="Q">The hex's axial <c>q</c>, as in <see cref="CombatMap.HexGrid" />.</param>
/// <param name="R">The hex's axial <c>r</c>, as in <see cref="CombatMap.HexGrid" />.</param>
/// <param name="Elevation">As in <see cref="HexRow.Elevations" />.</param>
/// <param name="Terrain">
///     Keyed as in <see cref="Catalog.TerrainDefinitions" />; <c>null</c> when on open ground.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MapHex(int Q, int R, float Elevation, string? Terrain);

/// <summary>A point on the map in meters: <see cref="Y" /> is the elevation.</summary>
/// <param name="X">Along the map's x axis, 0 at the map's center, to 0.1 m.</param>
/// <param name="Y">The elevation, on the scale of <see cref="HexRow.Elevations" />.</param>
/// <param name="Z">Along the map's z axis, 0 at the map's center, to 0.1 m.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MapPosition(float X, float Y, float Z);
