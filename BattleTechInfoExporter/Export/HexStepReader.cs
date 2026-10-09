using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using HBS.Math;
using UnityEngine;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Reads which steps from a hex to its neighbors the game's pathing blocks on slopes for the units of one group of
///     pathing capabilities. The game moves units over path nodes half a hex apart in eight directions (PathNodeGrid,
///     PathingUtil.GetForwardDelta), so a neighbor is two node steps away; the step to it is blocked when every such
///     route is. Terrain move costs and units in the way aren't part of it.
/// </summary>
internal sealed class HexStepReader
{
    // The cost PathNodeGrid gives a step it blocks (GetTerrainModifiedCost); no unit moves that far.
    private const float BlockedStepCost = 99999.9f;

    /// <summary>The six neighbors' offsets, in the game's direction order (HexPoint3.Step) and a mask's bit order.</summary>
    internal static readonly IReadOnlyList<HexPoint3> Directions =
        Enumerable.Range(0, 6).Select(direction => new HexPoint3(0, 0).Step(direction)).ToList();

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

    private readonly CombatGameState _combat;

    // Checks the ledges between two nodes, in both directions, as a grid of the group's units does.
    private readonly PathNodeGrid _ledgeChecker;

    // Every node step is shared by several routes, hexes and directions; the ledge check is the costly part.
    private readonly Dictionary<((int X, int Z) From, (int X, int Z) To), bool> _ledgesBetweenNodes = new();
    private readonly Dictionary<(int X, int Z), (Vector3 Position, MapTerrainDataCell Cell)> _nodes = new();

    internal HexStepReader(CombatGameState combat, PathingCapabilitiesDef pathing)
    {
        _combat = combat;
        // The grid only takes the battle from the unit; the ledge check reads nothing else of it.
        var unit = combat.AllActors.FirstOrDefault()
                   ?? throw new InvalidOperationException("The battle has no unit to check steps with");
        _ledgeChecker = new PathNodeGrid(unit)
        {
            Capabilities = pathing,
            // As PathNodeGrid.ResetPathGrid.
            maxGrade = pathing.MaxGrade * combat.Constants.MoveConstants.PathBlockerGradeMultiplier
        };
    }

    /// <summary>
    ///     The pathing capabilities the game has loaded, grouped by the limits that decide which steps they block, in
    ///     the order of their first id.
    /// </summary>
    internal static IReadOnlyList<IReadOnlyList<PathingCapabilitiesDef>> ReadPathingGroups(CombatGameState combat) =>
        combat.DataManager.PathingCapabilitiesDefs
            .Select(pathing => pathing.Value)
            .GroupBy(pathing => (pathing.MinGrade, pathing.MaxGrade, pathing.MaxSteepness, pathing.GradeMultiplier,
                pathing.GradeMultMaxAscending, pathing.GradeMultMaxDescending))
            .Select(group => group.OrderBy(pathing => pathing.Description.Id, StringComparer.Ordinal).ToList())
            .OrderBy(group => group[0].Description.Id, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    ///     The blocked steps from the hex as a mask, bit i set for <see cref="Directions" />[i]; a step to a hex
    ///     outside the playable area is blocked too.
    /// </summary>
    internal int ReadBlockedSteps(HexPoint3 hex, ISet<HexPoint3> playableHexes)
    {
        var start = NodeOf(hex);
        var mask = 0;
        for (var direction = 0; direction < Directions.Count; direction++)
        {
            var neighbor = hex.Step(direction);
            if (!playableHexes.Contains(neighbor) || !CanReach(start, NodeOf(neighbor), RouteNodes[direction]))
            {
                mask |= 1 << direction;
            }
        }

        return mask;
    }

    private bool CanReach((int X, int Z) start, (int X, int Z) end, List<(int X, int Z)> routeNodes)
    {
        foreach (var via in routeNodes)
        {
            var middle = (start.X + via.X, start.Z + via.Z);
            if (!IsStepBlocked(start, middle) && !IsStepBlocked(middle, end))
            {
                return true;
            }
        }

        return false;
    }

    // As PathNodeGrid.GetTerrainModifiedCost without the terrain's cost: too steep a grade or cell, or a ledge
    // between the nodes.
    private bool IsStepBlocked((int X, int Z) from, (int X, int Z) to)
    {
        var (fromPosition, _) = NodeAt(from);
        var (toPosition, toCell) = NodeAt(to);
        var distance = Vector2.Distance(
            new Vector2(fromPosition.x, fromPosition.z),
            new Vector2(toPosition.x, toPosition.z));
        var grade = PathingUtil.GetGrade(fromPosition.y, toPosition.y, distance);
        var cost = distance
                   * _ledgeChecker.GetGradeModifier(grade)
                   * _ledgeChecker.GetSteepnessMultiplier(toCell.cachedSteepness, grade);
        return cost >= BlockedStepCost || HasLedgeBetween(from, to);
    }

    // PathNodeGrid.FindBlockerReciprocal checks both directions, so one result serves both.
    private bool HasLedgeBetween((int X, int Z) from, (int X, int Z) to)
    {
        var nodePair = from.CompareTo(to) < 0 ? (from, to) : (to, from);
        if (!_ledgesBetweenNodes.TryGetValue(nodePair, out var hasLedge))
        {
            hasLedge = _ledgeChecker.FindBlockerReciprocal(NodeAt(from).Position, NodeAt(to).Position);
            _ledgesBetweenNodes.Add(nodePair, hasLedge);
        }

        return hasLedge;
    }

    // As PathNodeGrid.GetPathNode: a node is at the height of its cell.
    private (Vector3 Position, MapTerrainDataCell Cell) NodeAt((int X, int Z) node)
    {
        if (!_nodes.TryGetValue(node, out var position))
        {
            var halfHex = _combat.HexGrid.HexWidth / 2f;
            var ground = new Vector3(node.X * halfHex, 0f, node.Z * halfHex * HexGrid.SQRT_3_OVER_2);
            var cell = _combat.MapMetaData.GetCellAt(ground);
            position = (new Vector3(ground.x, cell.cachedHeight, ground.z), cell);
            _nodes.Add(node, position);
        }

        return position;
    }

    // Path nodes lie half a hex apart along x and half a row apart along z, with a hex's center on every node
    // (PathNodeGrid.ResetPathGrid, PathNode.IsLegalWorldPathLocationForActor).
    private static (int X, int Z) NodeOf(HexPoint3 hex) => (2 * hex.q + hex.r, 2 * hex.r);
}
