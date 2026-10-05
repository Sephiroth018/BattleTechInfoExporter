using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>Fires when an Argo upgrade is bought and its installation starts.</summary>
[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.QueueArgoUpgrade))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class ShipUpgradeStartedTrigger
{
    [HarmonyPostfix]
    private static void OnShipUpgradeStarted([HarmonyArgument("__instance")] SimGameState simGame)
    {
        GameStateExporter.Export(simGame, ExportTrigger.ShipUpgradeStarted);
    }
}
