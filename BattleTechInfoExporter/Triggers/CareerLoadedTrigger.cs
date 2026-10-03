using BattleTech;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires once the career is ready: after starting a new career, loading a save and returning from a
///     mission, right before the game publishes <see cref="SimGameUXAttached" />.
/// </summary>
[HarmonyPatch(typeof(SimGameState), nameof(SimGameState._OnAttachUXComplete))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class CareerLoadedTrigger
{
    [HarmonyPostfix]
    private static void OnCareerLoaded([HarmonyArgument("__instance")] SimGameState simGame)
    {
        GameStateExporter.Export(simGame, ExportTrigger.CareerLoaded);
    }
}
