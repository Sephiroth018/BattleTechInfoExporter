using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when the ship arrives in a star system after a trip, as the game shows the arrival popup or asks whether
///     to proceed with the travel contract.
/// </summary>
[HarmonyPatch(typeof(SGTravelManager), nameof(SGTravelManager.InSystem_OnEnter))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class ArrivedTrigger
{
    [HarmonyPrefix]
    private static void RememberArrival(
        [HarmonyArgument("__instance")] SGTravelManager travelManager,
        [HarmonyArgument("__state")] out bool isArriving)
    {
        // The state is also entered without a trip, e.g. when a career is loaded; the method itself resets the flag,
        // so a postfix alone can't tell.
        isArriving = travelManager.ArriveAtSystemFromTravel;
    }

    [HarmonyPostfix]
    private static void OnArrived(
        [HarmonyArgument("__instance")] SGTravelManager travelManager,
        [HarmonyArgument("__state")] bool isArriving)
    {
        if (isArriving)
        {
            GameStateExporter.Export(travelManager.simState, ExportTrigger.Arrived);
        }
    }
}
