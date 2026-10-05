using System;
using BattleTech;
using BattleTechInfoExporter.Export;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires once the after-action report has finalized a contract's salvage: after the priority salvage is
///     confirmed, or right away where there is nothing to choose. The salvage goes into storage only later, when the
///     contract's results are applied in the career screens.
/// </summary>
[HarmonyPatch(typeof(Contract), nameof(Contract.FinalizeSalvage))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class SalvageChosenTrigger
{
    [HarmonyPostfix]
    private static void OnSalvageChosen([HarmonyArgument("__instance")] Contract contract)
    {
        try
        {
            if (contract.SimGameContract)
            {
                MissionExporter.ExportSalvageReceived(contract.BattleTechGame.Simulation, contract);
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
