using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;
using StarSystem = BattleTech.StarSystem;

namespace BattleTechInfoExporter.Triggers;

/// <summary>Fires once the game has refreshed the star system the ship jumped into.</summary>
// SimGameState.SetCurrentSystem rolls the stores, then OnTargetSystemFound rolls them and the hiring hall again
// (StarSystem.RefreshSystem); only the second roll is what the player sees. It runs once the starmap's path finder
// completes, a few frames after the jump's day, or within the jump when the system is the story's TargetSystem.
[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.OnTargetSystemFound))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class JumpedTrigger
{
    // Systems rather than flags: a career loaded in between has new systems, so stale ones never match.
    private static StarSystem? _refreshedSystem;
    private static StarSystem? _jumpedIntoSystem;

    /// <summary>Exports the jump into the current system now if the game has refreshed it, or once it has.</summary>
    internal static void ExportOnceRefreshed(SimGameState simGame)
    {
        if (simGame.CurSystem == _refreshedSystem)
        {
            GameStateExporter.Export(simGame, ExportTrigger.Jumped);
        }
        else
        {
            _jumpedIntoSystem = simGame.CurSystem;
        }
    }

    [HarmonyPostfix]
    private static void OnTargetSystemFound([HarmonyArgument("__instance")] SimGameState simGame)
    {
        _refreshedSystem = simGame.CurSystem;
        if (simGame.CurSystem != _jumpedIntoSystem)
        {
            return;
        }

        _jumpedIntoSystem = null;
        GameStateExporter.Export(simGame, ExportTrigger.Jumped);
    }
}
