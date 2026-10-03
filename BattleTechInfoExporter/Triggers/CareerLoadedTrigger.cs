using System;
using BattleTech;
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
        // An exception escaping a patch would break the game's career loading; log it instead.
        try
        {
            ModLog.Logger.Log(
                $"Career loaded: company '{simGame.CompanyName}', system '{simGame.CurSystem.Name}'");
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
