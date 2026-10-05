using System;
using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires once the career is ready after starting a new career or loading a save, right before the game
///     publishes <see cref="SimGameUXAttached" />.
/// </summary>
[HarmonyPatch(typeof(SimGameState), nameof(SimGameState._OnAttachUXComplete))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class CareerLoadedTrigger
{
    [HarmonyPostfix]
    private static void OnCareerLoaded([HarmonyArgument("__instance")] SimGameState simGame)
    {
        try
        {
            // The catalog doesn't depend on the career; ExportFileWriter writes it once per game start.
            CatalogExporter.Export(simGame, ExportTrigger.CareerLoaded);
            // After a mission, the results are only applied a few frames later; ContractCompletedTrigger and
            // SaveTrigger export them.
            if (simGame.CompletedContract == null)
            {
                GameStateExporter.Export(simGame, ExportTrigger.CareerLoaded);
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
