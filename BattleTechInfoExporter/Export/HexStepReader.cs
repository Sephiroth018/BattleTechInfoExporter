using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using HBS.Math;
using UnityEngine;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Reads which steps from a hex to its neighbors the game's pathing blocks on slopes, per set of pathing
///     capabilities. The game moves units over path nodes half a hex apart in eight directions (PathNodeGrid,
///     PathingUtil.GetForwardDelta), so a neighbor is two node steps away; the step to it is blocked when every such
///     route is. Terrain move costs and units in the way aren't part of it.
/// </summary>
internal static class HexStepReader
{
    // The cost PathNodeGrid gives a step it blocks (GetTerrainModifiedCost); no unit moves that far.
    private const float BlockedStepCost = 99999.9f;

    /// <summary>The six neighbors' offsets in axial coordinates, in the order of a mask's bits.</summary>
    internal static readonly IReadOnlyList<HexPoint3> Directions =
    [
        new(1, 0), new(1, -1), new(0, -1), new(-1, 0), new(-1, 1), new(0, 1)
    ];

    // The eight path node steps (PathingUtil.GetForwardDelta), as (x, z) in path node units.
    private static readonly IReadOnlyList<(int X, int Z)> NodeSteps =
        [(0, 1), (1, 1), (1, 0), (1, -1), (0, -1), (-1, -1), (-1, 0), (-1, 1)];

    // Per direction, the path nodes a two-step route to the neighbor can pass through, as offsets from the hex.
    private static readonly List<List<(int X, int Z)>> RouteNodes = Directions
        .Select(direction =>
        {
            var target = NodeOf(direction);
            return NodeSteps
                .Where(step => NodeSteps.Contains((target.X - step.X, target.Z - step.Z)))
                .ToList();
        })
        .ToList();

    /// <summary>
    ///     The pathing capabilities the game has loaded, grouped by the limits that decide which steps they block, in
    ///     the order of their first id.
    /// </summary>
    internal static IReadOnlyList<IReadOnlyList<PathingCapabilitiesDef>> ReadPathingGroups(CombatGameState combat) =>
        combat.DataManager.PathingCapabilitiesDefs
            .Select(pathing => pathing.Value)
            .GroupBy(pathing => (pathing.MinGrade, pathing.MaxGrade, pathing.MaxSteepness, pathing.GradeMultiplier,
                pathing.GradeMultMaxAscending, pathing.GradeMultMaxDescending))
            .Select(group => (IReadOnlyList<PathingCapabilitiesDef>)group
                .OrderBy(pathing => pathing.Description.Id, StringComparer.Ordinal)
                .ToList())
            .OrderBy(group => group[0].Description.Id, StringComparer.Ordinal)
            .ToList();

    /// <summary>A path node grid that checks steps as the units of the pathing group do.</summary>
    internal static PathNodeGrid CreateStepChecker(CombatGameState combat, PathingCapabilitiesDef pathing)
    {
        // The grid only takes the battle from the unit; the blocker check reads nothing else of it.
        var unit = combat.AllActors.FirstOrDefault()
                   ?? throw new InvalidOperationException("The battle has no unit to check steps with");
        return new PathNodeGrid(unit)
        {
            Capabilities = pathing,
            // As PathNodeGrid.ResetPathGrid.
            maxGrade = pathing.MaxGrade * combat.Constants.MoveConstants.PathBlockerGradeMultiplier
        };
    }

    /// <summary>
    ///     The blocked steps from the hex as a mask, bit i set for <see cref="Directions" />[i]; a step to a hex
    ///     outside the playable area is blocked too.
    /// </summary>
    internal static int ReadBlockedSteps(
        CombatGameState combat,
        PathNodeGrid stepChecker,
        HexPoint3 hex,
        ISet<HexPoint3> playableHexes)
    {
        var start = NodeOf(hex);
        var mask = 0;
        for (var direction = 0; direction < Directions.Count; direction++)
        {
            var neighbor = new HexPoint3(hex.q + Directions[direction].q, hex.r + Directions[direction].r);
            var end = NodeOf(neighbor);
            var isBlocked = !playableHexes.Contains(neighbor)
                            || RouteNodes[direction].All(via =>
                            {
                                var middle = (start.X + via.X, start.Z + via.Z);
                                return IsStepBlocked(combat, stepChecker, start, middle)
                                       || IsStepBlocked(combat, stepChecker, middle, end);
                            });
            if (isBlocked)
            {
                mask |= 1 << direction;
            }
        }

        return mask;
    }

    // As PathNodeGrid.GetTerrainModifiedCost without the terrain's cost: too steep a grade or cell, or a ledge
    // between the nodes.
    private static bool IsStepBlocked(
        CombatGameState combat,
        PathNodeGrid stepChecker,
        (int X, int Z) from,
        (int X, int Z) to)
    {
        var fromPosition = PositionOf(combat, from);
        var toPosition = PositionOf(combat, to);
        var toCell = combat.MapMetaData.GetCellAt(toPosition);
        var distance = Vector2.Distance(
            new Vector2(fromPosition.x, fromPosition.z),
            new Vector2(toPosition.x, toPosition.z));
        var grade = PathingUtil.GetGrade(fromPosition.y, toPosition.y, distance);
        var cost = distance
                   * stepChecker.GetGradeModifier(grade)
                   * stepChecker.GetSteepnessMultiplier(toCell.cachedSteepness, grade);
        return cost >= BlockedStepCost || stepChecker.FindBlockerReciprocal(fromPosition, toPosition);
    }

    // Path nodes lie half a hex apart along x and half a row apart along z, with a hex's center on every node
    // (PathNodeGrid.ResetPathGrid, PathNode.IsLegalWorldPathLocationForActor).
    private static (int X, int Z) NodeOf(HexPoint3 hex) => (2 * hex.q + hex.r, 2 * hex.r);

    private static Vector3 PositionOf(CombatGameState combat, (int X, int Z) node)
    {
        var halfHex = combat.HexGrid.HexWidth / 2f;
        var position = new Vector3(node.X * halfHex, 0f, node.Z * halfHex * HexGrid.SQRT_3_OVER_2);
        // As PathNodeGrid.GetPathNode.
        position.y = combat.MapMetaData.GetCellAt(position).cachedHeight;
        return position;
    }
}
