using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when a phase has begun, before its first unit acts: the battle's start with the lance deployed, and
///     every later phase, also one reached by reserving the units of the one before.
/// </summary>
[HarmonyPatch(typeof(TurnDirector), nameof(TurnDirector.OnPhaseBeginComplete))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class PhaseStartedTrigger
{
    // A prefix: the method starts the phase's first activation (TurnDirector.IncrementActiveTurnActor).
    [HarmonyPrefix]
    private static void OnPhaseStarted([HarmonyArgument("__instance")] TurnDirector turnDirector) =>
        CombatExporter.Export(turnDirector.Combat, ExportTrigger.PhaseStarted);
}
