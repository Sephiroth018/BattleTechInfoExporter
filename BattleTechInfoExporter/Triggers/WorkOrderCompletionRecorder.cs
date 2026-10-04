using BattleTech;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Counts whole work orders as they complete: mech lab orders, heals in the medbay and Argo upgrades. The
///     triggers export once after the game code that completes them, when the count went up.
/// </summary>
[HarmonyPatch]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class WorkOrderCompletionRecorder
{
    // SimGameState.RefreshInjuries clears a pilot's injuries with this source only when their heal completes.
    private const string MedBaySource = "MedBay";

    /// <summary>Only ever increases, so a count taken before a call tells whether orders completed during it.</summary>
    internal static int CompletedCount { get; private set; }

    [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.CompleteWorkOrder))]
    [HarmonyPostfix]
    private static void RecordMechLabOrder(bool isSubEntry)
    {
        if (!isSubEntry)
        {
            CompletedCount++;
        }
    }

    [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.CompleteArgoUpgrade))]
    [HarmonyPostfix]
    private static void RecordArgoUpgrade()
    {
        CompletedCount++;
    }

    [HarmonyPatch(typeof(Pilot), nameof(Pilot.ClearInjuries))]
    [HarmonyPostfix]
    private static void RecordHeal([HarmonyArgument("sourceID")] string sourceId)
    {
        if (sourceId == MedBaySource)
        {
            CompletedCount++;
        }
    }
}
