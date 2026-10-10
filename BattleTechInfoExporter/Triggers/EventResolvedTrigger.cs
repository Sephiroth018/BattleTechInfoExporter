using System;
using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when the player closes an event's popup, once the picked option's results have changed the career
///     (SGEventPanel applies them before the popup can be closed).
/// </summary>
[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.OnEventDismissed))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class EventResolvedTrigger
{
    [HarmonyPostfix]
    private static void OnEventDismissed([HarmonyArgument("__instance")] SimGameState simGame)
    {
        try
        {
            GameStateExporter.Export(simGame, ExportTrigger.EventResolved);
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
