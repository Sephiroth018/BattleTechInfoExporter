using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires once a team's activation and its sequences (attacks, heat, the pilot's checks) are done, if a unit
///     finished its activation: after every unit in contact, after the player's whole move out of contact.
/// </summary>
[HarmonyPatch]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class ActivationCompletedTrigger
{
    private static bool _hasUnitFinished;

    // The game ends a unit's activation once a round: the method does nothing for a unit that has activated, and
    // reserving a unit doesn't call it (AbstractActor.DeferUnit). Mech.OnActivationEnd overrides it but calls it.
    [HarmonyPatch(typeof(AbstractActor), nameof(AbstractActor.OnActivationEnd))]
    [HarmonyPrefix]
    private static void RememberHadActivated(
        [HarmonyArgument("__instance")] AbstractActor actor,
        [HarmonyArgument("__state")] out bool hadActivated)
    {
        hadActivated = actor.HasActivatedThisRound;
    }

    [HarmonyPatch(typeof(AbstractActor), nameof(AbstractActor.OnActivationEnd))]
    [HarmonyPostfix]
    private static void RecordFinished(
        [HarmonyArgument("__instance")] AbstractActor actor,
        [HarmonyArgument("__state")] bool hadActivated)
    {
        _hasUnitFinished |= !hadActivated && actor.HasActivatedThisRound;
    }

    // A prefix: the method starts the next activation (TurnDirector.SendTurnActorActivateMessage).
    [HarmonyPatch(typeof(TurnDirector), nameof(TurnDirector.OnTurnActorActivateComplete))]
    [HarmonyPrefix]
    private static void OnActivationCompleted([HarmonyArgument("__instance")] TurnDirector turnDirector)
    {
        if (!_hasUnitFinished)
        {
            return;
        }

        _hasUnitFinished = false;
        CombatExporter.Export(turnDirector.Combat, ExportTrigger.ActivationCompleted);
    }
}
