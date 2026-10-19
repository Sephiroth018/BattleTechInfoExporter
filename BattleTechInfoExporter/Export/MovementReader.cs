using System;
using System.Collections.Generic;
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
    // A build returning fewer nodes than asked for has finished its grids (PathingManager.Update).
    private const int NodesPerBuild = 1000;

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

        var maxRanges = CombatUnitReader.ReadMaxRanges(actor);
        var hexes = new[] { walk, sprint, reverse, jump }
            .OfType<Dictionary<HexPoint3, int>>()
            .SelectMany(pips => pips.Keys)
            .Distinct()
            .OrderBy(hex => hex.r)
            .ThenBy(hex => hex.q)
            .Select(hex =>
            {
                var position = MapHexReader.StandingPosition(actor.Combat, hex);
                return new ReachableHex(
                    hex.q,
                    hex.r,
                    PipsAt(walk, hex),
                    PipsAt(sprint, hex),
                    PipsAt(reverse, hex),
                    PipsAt(jump, hex),
                    targets
                        .Select(target => ReadLineOfFire(actor, position, target, maxRanges))
                        .OfType<MoveLineOfFire>()
                        .ToList());
            })
            .ToList();
        return new UnitMovement(canSprint, jump is not null, hexes);
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

    // null where the move doesn't reach the hex or the unit can't make it.
    private static int? PipsAt(Dictionary<HexPoint3, int>? pips, HexPoint3 hex) =>
        pips is not null && pips.TryGetValue(hex, out var hexPips) ? hexPips : null;

    // null where the unit has no line of fire to the target and can't fire at it indirectly.
    private static MoveLineOfFire? ReadLineOfFire(
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
        var (blocking, fire, distance, elevationModifier) =
            CombatUnitReader.ReadFire(actor, level, position, target, maxRanges);
        return fire == FireAvailability.None
            ? null
            : new MoveLineOfFire(
                target.GUID,
                blocking,
                fire,
                distance,
                elevationModifier,
                ReadSide(actor, position, target));
    }

    // The side HitLocation.GetAttackDirection gives; a turret has a single location whichever it gives.
    private static AttackSide ReadSide(AbstractActor actor, Vector3 position, AbstractActor target)
    {
        if (target is Turret)
        {
            return AttackSide.Turret;
        }

        var direction = actor.Combat.HitLocation.GetAttackDirection(position, target);
        return direction switch
        {
            AttackDirection.FromFront => AttackSide.Front,
            AttackDirection.FromLeft => AttackSide.Left,
            AttackDirection.FromRight => AttackSide.Right,
            AttackDirection.FromBack => AttackSide.Rear,
            AttackDirection.ToProne => AttackSide.Prone,
            _ => throw new InvalidOperationException($"No side for the attack direction {direction}")
        };
    }
}
