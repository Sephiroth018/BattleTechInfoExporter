using System;
using BattleTech;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>Fires once the career screens are ready, right before the game publishes <see cref="SimGameUXAttached" />.</summary>
[HarmonyPatch(typeof(SimGameState), nameof(SimGameState._OnAttachUXComplete))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class CareerLoadedTrigger
{
    /// <summary>Subscribes to <see cref="SimGameUXAttached" />, if the game's message center exists yet.</summary>
    internal static void SubscribeToCareerScreensAttached()
    {
        var messageCenter = UnityGameInstance.BattleTechGame?.MessageCenter;
        ModEntryPoint.Logger.Log($"Message center available at mod load: {messageCenter is not null}");
        messageCenter?.AddSubscriber(MessageCenterMessageType.SimGameUXAttached, OnCareerScreensAttached);
    }

    [HarmonyPostfix]
    private static void OnCareerLoaded()
    {
        // An exception escaping a patch would break the game's career loading; log it instead.
        try
        {
            LogCareerLoaded("patch");
        }
        catch (Exception exception)
        {
            ModEntryPoint.Logger.LogException(exception);
        }
    }

    private static void OnCareerScreensAttached(MessageCenterMessage message)
    {
        LogCareerLoaded("message");
    }

    private static void LogCareerLoaded(string source)
    {
        var simGame = UnityGameInstance.BattleTechGame.Simulation;
        // UX_ATTACHED_PREVIOUSLY is set right after _OnAttachUXComplete returns, before the message is
        // published, so the patch and the message see different values for the same load.
        var hasAttachedBefore = simGame.HasInitStateBits(SimGameState.InitStates.UX_ATTACHED_PREVIOUSLY);
        ModEntryPoint.Logger.Log(
            $"Career loaded ({source}): company '{simGame.CompanyName}', system '{simGame.CurSystem.Name}', " +
            $"from save {simGame.IsFromSave}, screens attached before {hasAttachedBefore}");
    }
}
