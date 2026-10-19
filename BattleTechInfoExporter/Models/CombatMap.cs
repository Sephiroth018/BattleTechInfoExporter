using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The root of <c>combat-map.json</c>: the ground of the running battle on the game's movement hex grid, written
///     once when the battle begins; the file exists only while a battle is running.
/// </summary>
/// <param name="MapId">
///     The map's id in the game's metadata database, which keys <see cref="Catalog.MapDefinitions" />; a map no
///     star system's contracts are generated on, e.g. a story map, has no entry there. <c>null</c> when the map
///     isn't in the metadata database.
/// </param>
/// <param name="HexGrid">How the hexes' axial coordinates map to positions in meters, and their neighbors.</param>
/// <param name="PathingGroups">
///     The groups of the game's pathing capabilities units move with
///     (<see cref="ChassisMovement.PathingId" />) that block the same steps, by id;
///     <see cref="CombatMapHex.BlockedSteps" /> has an entry per group. The game's own data makes two groups, the
///     mechs' and the vehicles'.
/// </param>
/// <param name="Buildings">
///     Every building on the map when the file was written, in the order of their ids. A dropship occupies no hex
///     until it has landed; one landing later, e.g. the Leopard an evacuation calls in, isn't here but can be in the
///     combat state's building lists.
/// </param>
/// <param name="Hexes">
///     The hexes a unit can stand on, by <c>r</c>, then <c>q</c>; a hex that isn't listed is outside the playable
///     area.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatMap(
    string? MapId,
    HexGridLayout HexGrid,
    IReadOnlyList<IReadOnlyList<string>> PathingGroups,
    IReadOnlyList<CombatBuilding> Buildings,
    IReadOnlyList<CombatMapHex> Hexes) : ExportFile
{
    internal const string FileName = "combat-map.json";
}

/// <summary>How the hexes lie on the map, spelled out so the file explains itself.</summary>
/// <param name="Size">The distance between neighboring hex centers in meters.</param>
/// <param name="Orientation">The hexes' orientation: <c>pointyTop</c>, rows running along x.</param>
/// <param name="CenterFormula">The world position of a hex's center, as the game places units on it.</param>
/// <param name="Directions">
///     The six neighbors' offsets; <see cref="CombatMapHex.BlockedSteps" /> refers to them by their index here.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HexGridLayout(
    float Size,
    string Orientation,
    string CenterFormula,
    IReadOnlyList<HexOffset> Directions);

/// <summary>The step from a hex to one of its neighbors, in axial coordinates.</summary>
/// <param name="Q">The change of <c>q</c>.</param>
/// <param name="R">The change of <c>r</c>.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HexOffset(int Q, int R);

/// <summary>A building, which units can fire at, stand on and be blocked by.</summary>
/// <param name="Id">The game's building id, as in <see cref="CombatState.DamagedBuildings" />.</param>
/// <param name="Name">The building's name as the game shows it.</param>
/// <param name="Hex">
///     The hex the point the building is placed at lies in, which needn't be its center, a hex it covers or a
///     playable one; the hexes it covers name it in <see cref="CombatMapHex.Building" />.
/// </param>
/// <param name="MaxStructure">The structure of the undamaged building.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatBuilding(string Id, string Name, HexCoordinates Hex, float MaxStructure);

/// <summary>A hex a unit can stand on, as it is when the battle begins.</summary>
/// <param name="Building">
///     The <see cref="CombatBuilding.Id" /> of the building on the hex, which a unit standing there is on;
///     <c>null</c> when there is none.
/// </param>
/// <param name="BlockedSteps">
///     Per entry of <see cref="CombatMap.PathingGroups" />, the steps to neighbors its units can't take on slopes,
///     as indices into <see cref="HexGridLayout.Directions" />: too steep a grade between path nodes, too steep
///     ground or a ledge in between. A step to a hex that isn't listed is blocked too. Terrain move costs and units
///     in the way aren't part of it; impassable terrain is a terrain of its own. An approximation: a detour may still
///     reach a neighbor a blocked step can't; a player unit's <see cref="CombatUnit.Movement" /> is exact.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatMapHex(
    int Q,
    int R,
    float Elevation,
    string? Terrain,
    string? Building,
    IReadOnlyList<IReadOnlyList<int>> BlockedSteps) : MapHex(Q, R, Elevation, Terrain);
