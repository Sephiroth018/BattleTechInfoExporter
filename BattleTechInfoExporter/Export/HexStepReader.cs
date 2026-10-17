using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using HBS.Math;
using UnityEngine;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Reads which steps from a hex to its neighbors the game's pathing blocks on slopes for the units of one group of
///     pathing capabilities, approximated from the game's path node steps between the hexes, without terrain move
///     costs or units in the way.
/// </summary>
// Units pass any path node, half a hex apart (PathNodeGrid), and stop on hex centers. A step is open when node steps
// link the two centers through the two hexes and the two next to both; wider detours the game takes aren't searched.
internal sealed class HexStepReader
{
    // The cost PathNodeGrid gives a step it blocks (GetTerrainModifiedCost); no unit moves that far.
    private const float BlockedStepCost = 99999.9f;

    /// <summary>
    ///     The six neighbors' offsets, in the game's direction order (<see cref="HexPoint3.Step" />) and a mask's bit
    ///     order.
    /// </summary>
    internal static readonly IReadOnlyList<HexPoint3> Directions =
        Enumerable.Range(0, 6).Select(direction => new HexPoint3(0, 0).Step(direction)).ToList();

    // The eight path node steps (PathingUtil.GetForwardDelta), as (x, z) in path node units.
    private static readonly IReadOnlyList<(int X, int Z)> NodeSteps =
        [(0, 1), (1, 1), (1, 0), (1, -1), (0, -1), (-1, -1), (-1, 0), (-1, 1)];

    // A hex's nodes as offsets from its center: the center, the nodes halfway to its left and right neighbors, which
    // it shares with them, and the nodes half a row above and below, which lie inside it. Together they tile the map.
    private static readonly IReadOnlyList<(int X, int Z)> HexNodes = [(0, 0), (1, 0), (-1, 0), (0, 1), (0, -1)];

    // Per direction, the nodes a route to the neighbor may pass, as offsets from the hex's center: those of the hex,
    // the neighbor and the two hexes next to both, which lie in the directions before and after it.
    private static readonly List<HashSet<(int X, int Z)>> RouteNodes = Enumerable.Range(0, Directions.Count)
        .Select(direction => new HashSet<(int X, int Z)>(
            new[]
                {
                    new HexPoint3(0, 0),
                    Directions[direction],
                    Directions[(direction + 1) % Directions.Count],
                    Directions[(direction + Directions.Count - 1) % Directions.Count]
                }
                .SelectMany(hex => HexNodes.Select(node => Offset(NodeOf(hex), node)))))
        .ToList();

    // Every node step is shared by several routes, hexes and directions.
    private readonly Dictionary<((int X, int Z) From, (int X, int Z) To), bool> _blockedNodeSteps = new();

    private readonly CombatGameState _combat;

    // Checks the ledges between two nodes, in both directions, as a grid of the group's units does.
    private readonly PathNodeGrid _ledgeChecker;

    // The ledge check is the costly part.
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

    // A breadth-first search over the route's nodes, as PathNodeGrid.BuildPathNetwork spreads over every node.
    private bool CanReach((int X, int Z) start, (int X, int Z) end, HashSet<(int X, int Z)> routeNodes)
    {
        var reached = new HashSet<(int X, int Z)> { start };
        var open = new Queue<(int X, int Z)>();
        open.Enqueue(start);
        while (open.Count > 0)
        {
            var from = open.Dequeue();
            foreach (var step in NodeSteps)
            {
                var to = Offset(from, step);
                if (!routeNodes.Contains((to.X - start.X, to.Z - start.Z)) || reached.Contains(to)
                                                                           || IsStepBlocked(from, to))
                {
                    continue;
                }

                if (to == end)
                {
                    return true;
                }

                reached.Add(to);
                open.Enqueue(to);
            }
        }

        return false;
    }

    private bool IsStepBlocked((int X, int Z) from, (int X, int Z) to)
    {
        if (!_blockedNodeSteps.TryGetValue((from, to), out var isBlocked))
        {
            isBlocked = ReadIsStepBlocked(from, to);
            _blockedNodeSteps.Add((from, to), isBlocked);
        }

        return isBlocked;
    }

    // As PathNodeGrid.GetTerrainModifiedCost without the terrain's cost: too steep a grade or cell, or a ledge
    // between the nodes. A step leaving the map's cells is blocked, as PathNodeGrid.FindBlockerBetween treats one
    // starting outside them, whose check would otherwise read past the cells.
    private bool ReadIsStepBlocked((int X, int Z) from, (int X, int Z) to)
    {
        var (fromPosition, _) = NodeAt(from);
        var (toPosition, toCell) = NodeAt(to);
        if (!_combat.MapMetaData.IsWithinBounds(fromPosition) || !_combat.MapMetaData.IsWithinBounds(toPosition))
        {
            return true;
        }

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

    private static (int X, int Z) Offset((int X, int Z) node, (int X, int Z) offset) =>
        (node.X + offset.X, node.Z + offset.Z);
}
