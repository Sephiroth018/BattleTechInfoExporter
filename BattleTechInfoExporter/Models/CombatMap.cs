using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The root of <c>combat-map.json</c>: the ground of the running battle on the game's movement hex grid, written
///     once when the battle begins; the file exists only while a battle is running. Laid out in rows of characters
///     and numbers instead of an object per hex, so the file stays small enough for an AI chat to read whole.
/// </summary>
/// <param name="MapId">
///     The map's id in the game's metadata database, which keys <see cref="Catalog.MapDefinitions" />; a map no
///     star system's contracts are generated on, e.g. a story map, has no entry there. <c>null</c> when the map
///     isn't in the metadata database.
/// </param>
/// <param name="HexGrid">How the hexes' axial coordinates map to positions in meters, and their neighbors.</param>
/// <param name="Terrains">
///     The legend of <see cref="HexRow.Terrains" />: each character's terrain, keyed as in
///     <see cref="Catalog.TerrainDefinitions" />; <c>.</c> is open ground (<c>null</c>).
/// </param>
/// <param name="PathingGroups">
///     The groups of the game's pathing capabilities units move with
///     (<see cref="ChassisMovement.PathingId" />) that block the same steps, by id; <see cref="HexRow.BlockedSteps" />
///     has an entry per group. The game's own data makes two groups, the mechs' and the vehicles'.
/// </param>
/// <param name="Buildings">
///     Every building on the map when the file was written, in the order of their ids. A dropship occupies no hex
///     until it has landed; one landing later, e.g. the Leopard an evacuation calls in, isn't here but can be in the
///     combat state's building lists.
/// </param>
/// <param name="Rows">
///     The hexes a unit can stand on, by <c>r</c>, then <c>q</c>; a row of hexes with a gap in the playable area is
///     split in two, and the hexes outside it aren't listed.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatMap(
    string? MapId,
    HexGridLayout HexGrid,
    IReadOnlyDictionary<string, string?> Terrains,
    IReadOnlyList<IReadOnlyList<string>> PathingGroups,
    IReadOnlyList<CombatBuilding> Buildings,
    IReadOnlyList<HexRow> Rows) : ExportFile
{
    internal const string FileName = "combat-map.json";
}

/// <summary>How the hexes lie on the map, spelled out so the file explains itself.</summary>
/// <param name="Size">The distance between neighboring hex centers in meters.</param>
/// <param name="Orientation">The hexes' orientation: <c>pointyTop</c>, rows running along x.</param>
/// <param name="CenterFormula">The world position of a hex's center, as in the units' positions.</param>
/// <param name="Directions">The six neighbors' offsets, in the order of the bits of <see cref="HexRow.BlockedSteps" />.</param>
/// <param name="BlockedStepsFormula">How to read <see cref="HexRow.BlockedSteps" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HexGridLayout(
    float Size,
    string Orientation,
    string CenterFormula,
    IReadOnlyList<HexOffset> Directions,
    string BlockedStepsFormula);

/// <summary>The step from a hex to one of its neighbors, in axial coordinates.</summary>
/// <param name="Q">The change of <c>q</c>.</param>
/// <param name="R">The change of <c>r</c>.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HexOffset(int Q, int R);

/// <summary>A building, which units can fire at, stand on and be blocked by.</summary>
/// <param name="Id">The game's building id, as in <see cref="CombatState.DamagedBuildings" />.</param>
/// <param name="Name">The building's name as the game shows it.</param>
/// <param name="Position">The point the building is placed at, not necessarily its center.</param>
/// <param name="MaxStructure">The structure of the undamaged building.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatBuilding(string Id, string Name, MapPosition Position, float MaxStructure);

/// <summary>Consecutive hexes of one row, from (<see cref="Q" />, <see cref="R" />) on, one entry per hex in each list.</summary>
/// <param name="Q">The axial <c>q</c> of the first hex.</param>
/// <param name="R">The axial <c>r</c> of every hex in the row.</param>
/// <param name="Terrains">
///     Each hex's terrain as a character of <see cref="CombatMap.Terrains" />: the first that applies to its center
///     cell of map boundary, impassable, destroyed building, deep water, water, the terrain of the building on it,
///     road, the map's custom terrain, forest and rough ground; otherwise open ground.
/// </param>
/// <param name="Elevations">
///     The height in meters a unit standing on each hex is at, a building's roof where one stands, to 0.1 m.
/// </param>
/// <param name="Buildings">
///     The index in <see cref="CombatMap.Buildings" /> of the building on each hex, or <c>null</c>; that entry's
///     <see cref="CombatBuilding.Id" /> is what the combat state refers to the building by.
/// </param>
/// <param name="BlockedSteps">
///     Per entry of <see cref="CombatMap.PathingGroups" />, the steps to neighbors its units can't take on slopes,
///     one character per hex as <see cref="HexGridLayout.BlockedStepsFormula" /> says: too steep a grade between
///     path nodes, too steep ground or a ledge in between. Terrain move costs and units in the way aren't part of it;
///     impassable terrain has its own character in <see cref="CombatMap.Terrains" />. An approximation: a detour may
///     still reach a neighbor a blocked step can't; a player unit's <see cref="CombatUnit.Movement" /> is exact.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HexRow(
    int Q,
    int R,
    string Terrains,
    IReadOnlyList<float> Elevations,
    IReadOnlyList<int?> Buildings,
    IReadOnlyList<string> BlockedSteps);
