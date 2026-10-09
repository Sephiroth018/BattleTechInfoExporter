using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The root of <c>combat-map.json</c>: the ground of the running battle on the game's movement hex grid, written
///     once when the battle begins; the file exists only while a battle is running. Laid out in rows of characters
///     and numbers instead of an object per hex, so the file stays small enough for an AI chat to read whole.
/// </summary>
/// <param name="MapId">The map, keyed as in the catalog's <see cref="Catalog.MapDefinitions" />.</param>
/// <param name="Terrains">
///     The legend of <see cref="HexRow.Terrains" />: each character's terrain, keyed as in the catalog's
///     <see cref="Catalog.TerrainDefinitions" />; <c>.</c> is open ground (<c>null</c>).
/// </param>
/// <param name="PathingGroups">
///     The groups of the game's pathing capabilities units move with (the catalog's
///     <see cref="ChassisMovement.PathingId" />) that block the same steps, by id; <see cref="HexRow.BlockedSteps" />
///     has an entry per group.
/// </param>
/// <param name="Buildings">Every building on the map, in the order of their ids.</param>
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

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HexOffset(int Q, int R);

/// <summary>A building, which units can be fired at, stand on and be blocked by.</summary>
/// <param name="Id">The game's building id, as in <see cref="CombatState.DamagedBuildings" />.</param>
/// <param name="Position">The point the building is placed at, not necessarily its center.</param>
/// <param name="MaxStructure">The structure of the undamaged building.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatBuilding(string Id, string Name, MapPosition Position, float MaxStructure);

/// <summary>Consecutive hexes of one row, from (<see cref="Q" />, <see cref="R" />) on, one entry per hex in each list.</summary>
/// <param name="Q">The <c>q</c> of the first hex.</param>
/// <param name="Terrains">Each hex's terrain as a character of <see cref="CombatMap.Terrains" />.</param>
/// <param name="Elevations">
///     The height in meters a unit standing on each hex is at, a building's roof where one stands.
/// </param>
/// <param name="Buildings">The index in <see cref="CombatMap.Buildings" /> of the building on each hex, or <c>null</c>.</param>
/// <param name="BlockedSteps">
///     Per entry of <see cref="CombatMap.PathingGroups" />, the steps to neighbors its units can't take on slopes, one
///     character per hex as <see cref="HexGridLayout.BlockedStepsFormula" /> says.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HexRow(
    int R,
    int Q,
    string Terrains,
    IReadOnlyList<double> Elevations,
    IReadOnlyList<int?> Buildings,
    IReadOnlyList<string> BlockedSteps);
