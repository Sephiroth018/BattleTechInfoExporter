using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when the barracks confirm a pilot's training, the only time spent experience reaches the career.
/// </summary>
[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.UpgradePilot))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class ExperienceSpentTrigger
{
    [HarmonyPostfix]
    private static void OnExperienceSpent([HarmonyArgument("__instance")] SimGameState simGame)
    {
        GameStateExporter.Export(simGame, ExportTrigger.ExperienceSpent);
    }
}
