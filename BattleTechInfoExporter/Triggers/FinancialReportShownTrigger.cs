using BattleTech.UI;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when the monthly financial report is shown, so the spending level can be chosen with the export at hand.
/// </summary>
[HarmonyPatch(typeof(SimGameInterruptManager.FinancialReportEntry),
    nameof(SimGameInterruptManager.FinancialReportEntry.Render))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class FinancialReportShownTrigger
{
    [HarmonyPostfix]
    private static void OnFinancialReportShown(
        [HarmonyArgument("__instance")] SimGameInterruptManager.FinancialReportEntry financialReport)
    {
        GameStateExporter.Export(financialReport.manager.Sim, ExportTrigger.FinancialReportShown);
    }
}
