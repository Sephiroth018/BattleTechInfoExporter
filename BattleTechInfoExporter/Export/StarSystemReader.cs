using System;
using System.Collections.Generic;
using BattleTech;
using BattleTechInfoExporter.Models;
using Starmap = BattleTechInfoExporter.Models.Starmap;
using StarSystem = BattleTech.StarSystem;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the star systems file's model from the game's starmap.</summary>
internal static class StarSystemReader
{
    internal static Starmap Read(SimGameState simGame, ExportTrigger trigger)
    {
        var starSystems = new SortedDictionary<string, Models.StarSystem>(StringComparer.Ordinal);
        foreach (var system in simGame.StarSystems)
        {
            starSystems.Add(system.Def.Description.Id, ReadStarSystem(simGame, system));
        }

        return new Starmap(ModAssembly.Version, trigger, starSystems);
    }

    private static Models.StarSystem ReadStarSystem(SimGameState simGame, StarSystem system)
    {
        var canTravelTo = simGame.Starmap.CanTravelToNode(system.ID);
        // The starmap offers no trip to the current system (SGNavigationScreen.OnSystemRouted).
        var route = system.ID == simGame.CurSystem.ID ? (0, 0)
            : canTravelTo ? RouteReader.ReadRoute(simGame, system)
            : null;
        return new Models.StarSystem(
            system.Def.Description.Name,
            // As the starmap's system panel shows it (SGSystemViewPopulator).
            simGame.GetNormalizedDifficulty(system.Def),
            canTravelTo,
            route?.Days,
            route?.Cost);
    }
}
