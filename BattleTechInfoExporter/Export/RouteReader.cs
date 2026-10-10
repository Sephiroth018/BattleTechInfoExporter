using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using HBS.Nav;
using StarSystem = BattleTech.StarSystem;

namespace BattleTechInfoExporter.Export;

/// <summary>Reads the route the starmap plans from the current system to another.</summary>
internal static class RouteReader
{
    /// <summary>
    ///     The trip to another system, as the starmap shows it; <c>null</c> where the game finds no route, always for a
    ///     system whose travel requirements aren't met.
    /// </summary>
    internal static Route? ReadRoute(SimGameState simGame, StarSystem system)
    {
        // The path finder never enters a node whose travel requirements aren't met, the destination included
        // (StarSystemNode.GetConnectionAndCost), so the search would only fail.
        var starmap = simGame.Starmap;
        var destination = starmap.GetSystemByID(system.ID);
        if (!starmap.CanTravelToNode(destination))
        {
            return null;
        }

        // The game's Starmap.FindRouteTo steps the path finder over several frames; this steps an own one to the end,
        // which only computes.
        AStar.AStarResult? route = null;
        var pathFinder = new AStar.PathFinder();
        pathFinder.InitFindPath(
            starmap.GetSystemByID(simGame.CurSystem.ID),
            destination,
            1,
            1E-06f,
            result => route = result);
        while (pathFinder.Step())
        {
        }

        if (route is not { status: PathStatus.Complete })
        {
            ModLog.Logger.LogWarning($"Found no route to {system.ID}; its travel days and cost are left out");
            return null;
        }

        // Mirrors Starmap.OnPathfindingComplete.
        var nodes = route.path.Cast<StarSystemNode>().ToList();
        var jumps = nodes.Count - 1;
        return new Route(
            starmap.DistanceToJumpship()
            + nodes.Take(jumps).Sum(node => node.Cost)
            + nodes[jumps].System.JumpDistance,
            jumps * simGame.Constants.Finances.JumpShipCost);
    }
}
