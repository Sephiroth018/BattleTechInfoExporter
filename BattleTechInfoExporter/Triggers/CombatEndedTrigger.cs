using BattleTech;
using BattleTechInfoExporter.Export;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when the game tears a battle down (GameInstance.ClearCombat): when the after-action report is left after
///     the salvage, or when the battle is quit, restarted or left by loading a save.
/// </summary>
[HarmonyPatch(typeof(CombatGameState), nameof(CombatGameState.OnCombatGameDestroyed))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class CombatEndedTrigger
{
    [HarmonyPostfix]
    private static void OnCombatEnded() => CombatExporter.Delete();
}
