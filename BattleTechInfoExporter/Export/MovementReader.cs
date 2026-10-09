using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using HBS.Math;
using UnityEngine;
using Mech = BattleTech.Mech;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Reads where a player's unit can move from the path grids the game keeps for every unit (the AI's move
///     candidates, e.g. GenerateMoveCandidatesNode), and what it gets at each hex.
/// </summary>
internal static class MovementReader
{
    private const char NotReachedCharacter = '.';
    private const string NotReachedAttack = "..";

    // A build returning fewer nodes than asked for has finished its grids (PathingManager.Update).
    private const int NodesPerBuild = 1000;

    // An attack's first character; only direct fire tells how much its line of fire is blocked.
    private static readonly IReadOnlyDictionary<(FireAvailability Fire, LineOfFireBlocking? Blocking), AttackCode>
        FireCodes = new Dictionary<(FireAvailability Fire, LineOfFireBlocking? Blocking), AttackCode>
        {
            [(FireAvailability.Direct, LineOfFireBlocking.Clear)] =
                new('C', "direct fire, clear line of fire"),
            [(FireAvailability.Direct, LineOfFireBlocking.PartiallyBlocked)] =
                new('P', "direct fire, partially blocked line of fire"),
            [(FireAvailability.Indirect, null)] = new('I', "indirect fire only"),
            [(FireAvailability.OutOfRange, null)] =
                new('O', "line of fire, but beyond every weapon's range"),
            [(FireAvailability.None, null)] = new('-', "no fire")
        };

    // An attack's second character, the side HitLocation.GetAttackDirection gives.
    private static readonly IReadOnlyDictionary<AttackDirection, AttackCode> SideCodes =
        new Dictionary<AttackDirection, AttackCode>
        {
            [AttackDirection.FromFront] = new('F', "front"),
            [AttackDirection.FromLeft] = new('L', "left"),
            [AttackDirection.FromRight] = new('R', "right"),
            [AttackDirection.FromBack] = new('B', "rear"),
            [AttackDirection.ToProne] = new('P', "a prone target, hit anywhere")
        };

    private static readonly AttackCode TurretSide = new('.', "a turret, which has a single location");

    internal static readonly MovementLegend Legend = new(
        $"one character per hex: the evasion pips a move ending there gives; {NotReachedCharacter} where the move "
        + $"doesn't reach it, an attack there is {NotReachedAttack} then",
        ReadLegend(FireCodes.Values),
        ReadLegend(SideCodes.Values.Append(TurretSide)));

    /// <summary>The unit's movement; <c>null</c> for a unit that can't move.</summary>
    internal static UnitMovement? Read(AbstractActor actor, IReadOnlyList<AbstractActor> targets)
    {
        if (actor.Pathing is not { } pathing || !actor.CanMove)
        {
            return null;
        }

        // The game builds the grids over several frames after it resets them (AbstractActor.ResetPathing), as when
        // an activation ends; finishing them now is the work it would do next.
        while (pathing.UpdateBuild(NodesPerBuild) == NodesPerBuild)
        {
        }

        var walk = ReadGridPips(actor, MoveType.Walking);
        // Mech.CanSprint without having fired this round, which only a unit that has activated has, until the next.
        var canSprint = actor is Mech mech ? !mech.IsLegged && !mech.IsUnsteady : actor.CanSprint;
        var sprint = canSprint ? ReadGridPips(actor, MoveType.Sprinting) : null;
        var reverse = ReadGridPips(actor, MoveType.Backward);
        var jump = actor is Mech { JumpDistance: > 0f } jumpingMech ? ReadJumpPips(jumpingMech) : null;

        var hexes = new[] { walk, sprint, reverse, jump }
            .OfType<Dictionary<HexPoint3, int>>()
            .SelectMany(pips => pips.Keys)
            .Distinct()
            .ToList();
        var maxRanges = CombatUnitReader.ReadMaxRanges(actor);
        var attacks = hexes.ToDictionary(
            hex => hex,
            hex =>
            {
                var position = MapHexReader.StandingPosition(actor.Combat, hex);
                return targets.Select(target => ReadAttack(actor, position, target, maxRanges)).ToList();
            });

        var rows = hexes
            .GroupBy(hex => hex.r)
            .OrderBy(row => row.Key)
            .Select(row =>
            {
                var firstQ = row.Min(hex => hex.q);
                var rowHexes = Enumerable.Range(firstQ, row.Max(hex => hex.q) - firstQ + 1)
                    .Select(q => new HexPoint3(q, row.Key))
                    .ToList();
                return new MovementRow(
                    row.Key,
                    firstQ,
                    ReadMoveRow(rowHexes, walk),
                    ReadMoveRow(rowHexes, sprint),
                    ReadMoveRow(rowHexes, reverse),
                    ReadMoveRow(rowHexes, jump),
                    Enumerable.Range(0, targets.Count)
                        .Select(targetIndex => string.Concat(rowHexes.Select(hex =>
                            attacks.TryGetValue(hex, out var hexAttacks) ? hexAttacks[targetIndex] : NotReachedAttack)))
                        .ToList());
            })
            .ToList();
        return new UnitMovement(targets.Select(target => target.GUID).ToList(), rows);
    }

    // The destinations the move preview offers, each with the length of the path the unit would walk, as the HUD
    // previews it and the move applies it (CombatHUDStatusPanel, Mech.OnMoveOrSprintComplete).
    private static Dictionary<HexPoint3, int> ReadGridPips(AbstractActor actor, MoveType moveType)
    {
        var grid = actor.Pathing.getGrid(moveType);
        var pips = new Dictionary<HexPoint3, int>();
        foreach (var node in grid.GetSampledPathNodes()
                     .Where(node => IsAllowedDestination(actor, node.Position)))
        {
            var path = grid.BuildPathFromEnd(
                node,
                grid.MaxDistance,
                node.Position,
                node.Position,
                null,
                out _,
                out var destination,
                out _);
            var distance = WayPoint.GetDistFromWaypointList(
                actor.CurrentPosition,
                ActorMovementSequence.ExtractWaypointsFromPath(actor, path, destination, null, moveType));
            AddPips(
                pips,
                actor.Combat.HexGrid.GetClosestHexPoint3OnGrid(node.Position),
                actor.GetEvasivePipsResult(distance, false, moveType == MoveType.Sprinting, false));
        }

        return pips;
    }

    // JumpPathing.GetHexGridSampledPathNodes, with each hex's height set before checking the jump distance: the game
    // checks it at height 0, which drops every hex on a map higher than the mech can jump. The landing spot check
    // compares the real heights, as the HUD's jump does (JumpPathing.GetValidJumpTarget), which also leaves out
    // destroyed units. A jump's distance is the straight one (Mech.OnJumpComplete).
    private static Dictionary<HexPoint3, int> ReadJumpPips(Mech mech)
    {
        var combat = mech.Combat;
        var otherUnits = combat.AllActors.Where(actor => actor != mech && !actor.IsDead).ToList();
        var radius = Mathf.CeilToInt(1.5f * mech.JumpDistance / combat.HexGrid.HexWidth);
        var pips = new Dictionary<HexPoint3, int>();
        foreach (var point in combat.HexGrid.GetGridPointsAroundPointWithinRadius(mech.CurrentPosition, radius))
        {
            // Heights only lengthen the distance, so a hex beyond the jump distance on the flat is beyond it anyway.
            var flatOffset = point - mech.CurrentPosition;
            flatOffset.y = 0f;
            if (flatOffset.magnitude > mech.JumpDistance)
            {
                continue;
            }

            var landing = point;
            landing.y = combat.MapMetaData.GetLerpedHeightAt(point);
            if (combat.MapMetaData.IsWithinBounds(landing)
                && PathNode.IsLegalWorldPathLocationForActor(landing, mech)
                && mech.JumpPathing.IsValidLandingSpot(landing, otherUnits)
                && IsAllowedDestination(mech, landing))
            {
                AddPips(
                    pips,
                    combat.HexGrid.GetClosestHexPoint3OnGrid(landing),
                    mech.GetEvasivePipsResult(Vector3.Distance(mech.CurrentPosition, landing), true, false, false));
            }
        }

        return pips;
    }

    // A destination the HUD lets the player confirm (SelectionStateMove, SelectionStateJump): a unit scripted to stay
    // in a region can't end its move outside it, and one restricted to melee can't move without a target.
    private static bool IsAllowedDestination(AbstractActor actor, Vector3 position) =>
        actor.IsAllowedToMoveToPosition(position, null);

    // Only hex centers are destinations inside the encounter bounds (PathNode.IsLegalWorldPathLocationForActor);
    // outside them, where every path node is, the nodes of a hex count once, with the fewest pips.
    private static void AddPips(Dictionary<HexPoint3, int> pips, HexPoint3 hex, int hexPips) =>
        pips[hex] = pips.TryGetValue(hex, out var otherPips) ? Math.Min(otherPips, hexPips) : hexPips;

    // null for a move the unit can't make.
    [return: NotNullIfNotNull(nameof(pips))]
    private static string? ReadMoveRow(IEnumerable<HexPoint3> rowHexes, Dictionary<HexPoint3, int>? pips) =>
        pips is null
            ? null
            : string.Concat(rowHexes.Select(hex =>
                pips.TryGetValue(hex, out var hexPips) ? ToDigit(hexPips) : NotReachedCharacter));

    // Pips are capped by the MaxEvasivePips statistic, a single digit in the game's data.
    private static char ToDigit(int pips) =>
        pips is >= 0 and <= 9
            ? (char)('0' + pips)
            : throw new InvalidOperationException($"{pips} evasion pips don't fit a single digit");

    private static string ReadAttack(
        AbstractActor actor,
        Vector3 position,
        AbstractActor target,
        (float Direct, float Indirect) maxRanges)
    {
        // As the HUD's move preview reads it (FiringPreviewManager), from the game's cache, kept for the whole phase.
        var level = actor.Combat.LOS.GetLineOfFire(
            actor,
            position,
            target,
            target.CurrentPosition,
            target.CurrentRotation,
            out _);
        var (blocking, fire) = CombatUnitReader.ReadFire(level, position, target, maxRanges);
        var fireCode = FireCodes[(fire, fire == FireAvailability.Direct ? blocking : null)];
        var direction = actor.Combat.HitLocation.GetAttackDirection(position, target);
        var sideCode = target is Turret
            ? TurretSide
            : SideCodes.TryGetValue(direction, out var code)
                ? code
                : throw new InvalidOperationException($"No side for the attack direction {direction}");
        return $"{fireCode.Character}{sideCode.Character}";
    }

    private static SortedDictionary<string, string> ReadLegend(IEnumerable<AttackCode> codes) =>
        new(
            codes.ToDictionary(code => code.Character.ToString(), code => code.Description),
            StringComparer.Ordinal);

    private sealed record AttackCode(char Character, string Description);
}
