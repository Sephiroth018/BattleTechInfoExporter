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
/// <param name="Units">
///     Every unit the player knows of, keyed by the game's unit id: the player's and allied units, and the enemies
///     and neutral units the HUD shows or last showed. A destroyed unit stays, as its wreck is on the map; an enemy or
///     neutral unit that withdrew or that the mission's script removed leaves no wreck and is left out.
/// </param>
/// <param name="Objectives">The objectives the HUD lists, finished ones included, in the HUD's order.</param>
/// <param name="Zones">The zones drawn on the map.</param>
/// <param name="LandingZones">
///     Where a dropship or drop pods are about to land; entries come and go as the mission readies landings and
///     they happen. The game ties none to the objective announcing it: match them by hex with the
///     objective's <c>beacon</c>. A lance arriving without drop pods or a dropship has none; the band along the
///     encounter boundary, which the HUD marks the same way, isn't exported.
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
///     The hexes whose center cell the game marks, by <c>r</c>, then <c>q</c>, as in <see cref="CombatMap.Hexes" />;
///     a unit standing on one is crushed. A dropship's whole footprint, or each drop pod's hex, one per unit of the
///     lance.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LandingZone(string Id, LandingZoneKind Kind, IReadOnlyList<HexCoordinates> Hexes);

/// <summary>A hex of the game's movement grid by its axial coordinates, as in <see cref="CombatMap.Hexes" />.</summary>
/// <param name="Q">The hex's axial <c>q</c>, as in <see cref="CombatMap.HexGrid" />.</param>
/// <param name="R">The hex's axial <c>r</c>, as in <see cref="CombatMap.HexGrid" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal record HexCoordinates(int Q, int R);

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
///     The hexes the building stood on, now with the ground's elevation and the rubble's terrain, since
///     <c>combat-map.json</c> isn't rewritten when it falls; units on its roof drop to the ground. <c>null</c> when
///     it stands. The blocked steps onto and off them still follow the roof, and the terrain a building's explosion
///     changes around it isn't exported.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record DamagedBuilding(string Id, float Structure, IReadOnlyList<MapHex>? DestroyedHexes);

/// <summary>A hex of <see cref="CombatMap.Hexes" /> with its ground as it is now.</summary>
/// <param name="Elevation">
///     The height in meters a unit standing on the hex is at, a building's roof where one stands, to 0.1 m.
/// </param>
/// <param name="Terrain">
///     The terrain a unit standing on the hex is in, keyed as in <see cref="Catalog.TerrainDefinitions" />: the
///     first that applies to its center cell of map boundary, impassable, destroyed building, deep water, water, the
///     terrain of the building on it, road, the map's custom terrain, forest and rough ground; <c>null</c> when on
///     open ground.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal record MapHex(int Q, int R, float Elevation, string? Terrain) : HexCoordinates(Q, R);
