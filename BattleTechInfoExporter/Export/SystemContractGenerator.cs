using BattleTech;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Generates a star system's contracts the way the Command Center's contract screen does when it first opens
///     there.
/// </summary>
internal static class SystemContractGenerator
{
    /// <summary>
    ///     Generates the current system's contracts when the game hasn't yet, so every career export in a system has
    ///     them. Like opening the screen, it changes the career: the contracts land in the next save.
    /// </summary>
    internal static void GenerateMissingContracts(SimGameState simGame)
    {
        // The screen can't open before the UX is attached or during a trip (SGTravelManager.HandleNextTravelStep).
        if (!simGame.UXAttached
            || simGame.TravelState != SimGameTravelStatus.IN_SYSTEM
            || simGame.CurSystem.InitialContractsFetched)
        {
            return;
        }

        Generate(simGame.CurSystem);
        // Not OnInitialContractFetched: it fires ContractsGeneratedTrigger, a second export inside this one.
        simGame.CurSystem.initialContractsFetched = true;
    }

    /// <summary>
    ///     StarSystem.GenerateInitialContracts' call without its callback and its useCoroutine, which waits 0.2 s per
    ///     contract for the screen's loading spinner, so it completes within the call.
    /// </summary>
    internal static void Generate(StarSystem system) => system.Sim.GeneratePotentialContracts(true, null);
}
