using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when the battle's first phase has begun, with the lance deployed and before the first unit acts.
/// </summary>
[HarmonyPatch(typeof(TurnDirector), nameof(TurnDirector.OnPhaseBeginComplete))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class CombatStartedTrigger
{
    // A prefix: the method starts the phase's first activation (TurnDirector.IncrementActiveTurnActor).
    [HarmonyPrefix]
    private static void OnCombatStarted([HarmonyArgument("__instance")] TurnDirector turnDirector)
    {
        if (turnDirector.CurrentRound == 1 && turnDirector.CurrentPhase == turnDirector.FirstPhase)
        {
            CombatExporter.Export(turnDirector.Combat, ExportTrigger.CombatStarted);
        }
    }
}
