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
    // A prefix: without an intro, the method starts the first round (CheckCameraIntroAndDropshipComplete), whose
    // combat state refers to the map.
    [HarmonyPrefix]
    private static void OnCombatBegin([HarmonyArgument("__instance")] TurnDirector turnDirector)
    {
        var combat = turnDirector.Combat;
        CombatExporter.ExportMap(
            combat,
            combat.WasFromSave ? ExportTrigger.CombatLoaded : ExportTrigger.CombatStarted);
    }

    // A postfix: the method restores the loaded battle's visibility.
    [HarmonyPostfix]
    private static void OnCombatBegun([HarmonyArgument("__instance")] TurnDirector turnDirector)
    {
        if (turnDirector.Combat.WasFromSave)
        {
            CombatExporter.Export(turnDirector.Combat, ExportTrigger.CombatLoaded);
        }
    }
}
