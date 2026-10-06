using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;
using StarSystem = BattleTech.StarSystem;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when the game finishes generating a system's initial contracts, which it only does when the Command
///     Center's contract screen first opens there, or after a contract.
/// </summary>
[HarmonyPatch(typeof(StarSystem), nameof(StarSystem.OnInitialContractFetched))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class ContractsGeneratedTrigger
{
    [HarmonyPostfix]
    private static void OnContractsGenerated([HarmonyArgument("__instance")] StarSystem system)
    {
        GameStateExporter.Export(system.Sim, ExportTrigger.ContractsGenerated);
    }
}
