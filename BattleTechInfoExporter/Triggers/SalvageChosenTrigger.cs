using System;
using System.Linq;
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
    // Taken before the game moves the chosen items out of the pool into SalvageResults. GetPotentialSalvage
    // returns copies; SalvageResults is the list the game appends to, so it's copied. A skirmish's contract has
    // neither.
    [HarmonyPrefix]
    private static void RememberSalvageOffer(
        [HarmonyArgument("__instance")] Contract contract,
        [HarmonyArgument("__state")] out SalvageOffer? offer)
    {
        offer = null;
        try
        {
            if (contract.SimGameContract)
            {
                offer = new SalvageOffer(contract.GetPotentialSalvage(), contract.SalvageResults.ToList());
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }

    [HarmonyPostfix]
    private static void OnSalvageChosen(
        [HarmonyArgument("__instance")] Contract contract,
        [HarmonyArgument("__state")] SalvageOffer? offer)
    {
        try
        {
            // Null for a skirmish's contract, or when reading the offer failed, which the prefix has logged.
            if (offer != null)
            {
                MissionExporter.ExportSalvageReceived(contract.BattleTechGame.Simulation, contract, offer);
            }
        }
        catch (Exception exception)
        {
            ModLog.Logger.LogException(exception);
        }
    }
}
