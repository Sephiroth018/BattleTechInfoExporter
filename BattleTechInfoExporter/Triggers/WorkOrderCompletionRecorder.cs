using BattleTech;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;
using Pilot = BattleTech.Pilot;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Records whole work orders as they complete: mech lab orders, heals in the medbay and ship upgrades. The
///     triggers export once after the game code that completes them, when orders completed during it, with the
///     trigger of the order completed last.
/// </summary>
[HarmonyPatch]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class WorkOrderCompletionRecorder
{
    // SimGameState.RefreshInjuries clears a pilot's injuries with this source only when their heal completes.
    private const string MedBaySource = "MedBay";

    private static ExportTrigger _lastCompletedTrigger;

    /// <summary>Only ever increases, so a count taken before a call tells whether orders completed during it.</summary>
    internal static int CompletedCount { get; private set; }

    /// <summary>
    ///     The trigger of the order completed last since the count was <paramref name="completedCountBefore" />,
    ///     or <c>null</c> when none completed since.
    /// </summary>
    internal static ExportTrigger? CompletedSince(int completedCountBefore) =>
        CompletedCount != completedCountBefore ? _lastCompletedTrigger : null;

    [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.CompleteWorkOrder))]
    [HarmonyPostfix]
    private static void RecordMechLabOrder(bool isSubEntry)
    {
        if (!isSubEntry)
        {
            Record(ExportTrigger.MechLabOrderCompleted);
        }
    }

    [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.CompleteArgoUpgrade))]
    [HarmonyPostfix]
    private static void RecordShipUpgrade() => Record(ExportTrigger.ShipUpgradeCompleted);

    [HarmonyPatch(typeof(Pilot), nameof(Pilot.ClearInjuries))]
    [HarmonyPostfix]
    private static void RecordHeal([HarmonyArgument("sourceID")] string sourceId)
    {
        if (sourceId == MedBaySource)
        {
            Record(ExportTrigger.PilotHealed);
        }
    }

    private static void Record(ExportTrigger trigger)
    {
        CompletedCount++;
        _lastCompletedTrigger = trigger;
    }
}
