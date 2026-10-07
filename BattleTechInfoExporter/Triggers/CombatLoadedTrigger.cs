using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when a battle is loaded from a save, once its units and visibility are restored; such a battle
///     continues where it was saved, without <see cref="CombatStartedTrigger" />.
/// </summary>
[HarmonyPatch(typeof(TurnDirector), nameof(TurnDirector.OnEncounterBegin))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class CombatLoadedTrigger
{
    [HarmonyPostfix]
    private static void OnCombatLoaded([HarmonyArgument("__instance")] TurnDirector turnDirector)
    {
        if (turnDirector.Combat.WasFromSave)
        {
            CombatExporter.Export(turnDirector.Combat, ExportTrigger.CombatLoaded);
        }
    }
}
