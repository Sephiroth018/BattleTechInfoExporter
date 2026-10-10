using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The root of <c>combat-state.json</c>: the battle as the player's HUD shows it, written when every phase
///     begins and after every unit's activation; the file exists only while a battle is running.
/// </summary>
/// <param name="Contract">The contract being fought, as in <see cref="MissionOutcome.Contract" />.</param>
/// <param name="MapId">The map, keyed as in the catalog's <see cref="Catalog.MapDefinitions" />.</param>
/// <param name="Round">The combat round, counted from 1.</param>
/// <param name="Phase">
///     The current phase as the HUD numbers it, as <see cref="CombatUnitState.Initiative" />: units act from 5
///     down to 1.
/// </param>
/// <param name="Resolve">The lance's resolve now; its limits are in <c>rules.json</c>'s <see cref="ResolveRules" />.</param>
/// <param name="MovementLegend">How to read the units' <see cref="CombatUnit.Movement" />.</param>
/// <param name="Units">
///     Every unit the player knows of, keyed by the game's unit id: the player's and allied units, and the enemies
///     and neutral units the HUD shows or last showed.
/// </param>
/// <param name="Objectives">The objectives the HUD lists, finished ones included, in the HUD's order.</param>
/// <param name="Zones">The zones drawn on the map.</param>
/// <param name="BuildingSides">
///     The buildings on a side, e.g. a base to destroy or defend, by their id in <see cref="CombatMap.Buildings" />
///     and in the order of their ids; the others are on none. The mission's script can change a building's side
///     during the battle.
/// </param>
/// <param name="DamagedBuildings">
///     The buildings of <see cref="CombatMap.Buildings" /> below their max structure, destroyed ones included, in
///     the order of their ids; the others are undamaged.
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
    IReadOnlyList<BuildingSide> BuildingSides,
    IReadOnlyList<DamagedBuilding> DamagedBuildings) : ExportFile
{
    internal const string FileName = "combat-state.json";
}

/// <summary>A building on a side, highlighted in its team's color on the map.</summary>
/// <param name="Id">The building's id in <see cref="CombatMap.Buildings" />.</param>
/// <param name="Faction">The faction of the building's team.</param>
/// <param name="Allegiance">Whose side the building is on, seen from the player.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record BuildingSide(string Id, DefinitionReference Faction, UnitAllegiance Allegiance);

/// <param name="Id">The building's id in <see cref="CombatMap.Buildings" />.</param>
/// <param name="Structure">The structure left; its max is in <see cref="CombatBuilding.MaxStructure" />.</param>
/// <param name="DestroyedHexes">
///     The hexes the building stood on as they are now, since <c>combat-map.json</c> isn't rewritten when it falls:
///     units on its roof drop to the ground, which the rubble's terrain covers. <c>null</c> while it stands.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record DamagedBuilding(string Id, float Structure, IReadOnlyList<MapHex>? DestroyedHexes);

/// <summary>A hex of <see cref="CombatMap.Rows" /> as it is now.</summary>
/// <param name="Q">The hex's axial <c>q</c>, as in <see cref="CombatMap.HexGrid" />.</param>
/// <param name="R">The hex's axial <c>r</c>, as in <see cref="CombatMap.HexGrid" />.</param>
/// <param name="Elevation">As in <see cref="HexRow.Elevations" />.</param>
/// <param name="Terrain">
///     Keyed as in the catalog's <see cref="Catalog.TerrainDefinitions" />; <c>null</c> on open ground.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MapHex(int Q, int R, double Elevation, string? Terrain);

/// <summary>A point on the map in meters: <see cref="Y" /> is the elevation.</summary>
/// <param name="X">Along the map's x axis, 0 at the map's center.</param>
/// <param name="Y">The elevation, on the scale of <see cref="HexRow.Elevations" />.</param>
/// <param name="Z">Along the map's z axis, 0 at the map's center.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MapPosition(float X, float Y, float Z);
