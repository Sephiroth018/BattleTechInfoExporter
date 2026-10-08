using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when a battle begins, once its map, buildings and units are in place: a new one when the briefing is
///     dismissed, before its first phase, and one loaded from a save once its units and visibility are restored;
///     such a battle continues mid-phase, without <see cref="PhaseStartedTrigger" />.
/// </summary>
[HarmonyPatch(typeof(TurnDirector), nameof(TurnDirector.OnEncounterBegin))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class CombatBegunTrigger
{
    [HarmonyPostfix]
    private static void OnCombatBegun([HarmonyArgument("__instance")] TurnDirector turnDirector)
    {
        var combat = turnDirector.Combat;
        if (combat.WasFromSave)
        {
            CombatExporter.ExportMap(combat, ExportTrigger.CombatLoaded);
            CombatExporter.Export(combat, ExportTrigger.CombatLoaded);
        }
        else
        {
            CombatExporter.ExportMap(combat, ExportTrigger.CombatStarted);
        }
    }
}
