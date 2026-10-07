using BattleTech;
using BattleTechInfoExporter.Export;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when the game tears a battle down (GameInstance.ClearCombat): once its salvage is final, or when it is
///     quit, restarted or left by loading a save.
/// </summary>
[HarmonyPatch(typeof(CombatGameState), nameof(CombatGameState.OnCombatGameDestroyed))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class CombatEndedTrigger
{
    [HarmonyPostfix]
    private static void OnCombatEnded() => CombatExporter.Delete();
}
