using System;
using BattleTech;
using BattleTech.Save.SaveGameStructure;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires on every career save, manual or automatic, once the game allowed it; the career is serialized
///     synchronously inside <see cref="SaveGameStructure.Save" />, so the export matches the save.
/// </summary>
[HarmonyPatch(typeof(SaveGameStructure), nameof(SaveGameStructure.Save))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class SaveTrigger
{
    [HarmonyPostfix]
    private static void OnSaved(GameInstance gameInstance, SaveReason saveReason)
    {
        try
        {
            // A save during combat holds the career state from before the mission.
            if (gameInstance.Simulation == null || gameInstance.Combat != null)
            {
                return;
            }

            var trigger = saveReason == SaveReason.MANUAL ? ExportTrigger.ManualSave : ExportTrigger.Autosave;
            GameStateExporter.Export(gameInstance.Simulation, trigger);
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
