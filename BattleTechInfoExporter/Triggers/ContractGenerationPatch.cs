using System;
using BattleTechInfoExporter.Export;
using HarmonyLib;
using JetBrains.Annotations;
using StarSystem = BattleTech.StarSystem;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Makes the game generate a system's contracts within the call when the contract screen first opens there or
///     after a contract: the screen opens at once, and no generation is under way while the mod exports.
/// </summary>
[HarmonyPatch(typeof(StarSystem), nameof(StarSystem.GenerateInitialContracts))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class ContractGenerationPatch
{
    [HarmonyPrefix]
    private static bool GenerateAtOnce([HarmonyArgument("__instance")] StarSystem system, Action? onContractsFetched)
    {
        try
        {
            ContractGenerator.Generate(system);
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
            // The game's coroutine starts over from an empty list.
            return true;
        }

        // The game's completion step, which calls the callback: an exception in there is the caller's.
        system.contractRetrievalCallback = onContractsFetched;
        system.OnInitialContractFetched();
        return false;
    }
}
