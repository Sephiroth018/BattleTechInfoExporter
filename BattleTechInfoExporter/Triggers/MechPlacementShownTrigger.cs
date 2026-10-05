using BattleTech.UI;
using BattleTechInfoExporter.Export;
using BattleTechInfoExporter.Models;
using HarmonyLib;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Triggers;

/// <summary>
///     Fires when the game asks where to put a new mech because every mech bay is full. After a contract, the
///     game's autosave waits until the dialog is closed, so without this the export would miss the contract's
///     results while the player decides.
/// </summary>
[HarmonyPatch(
    typeof(MechPlacementPopup),
    nameof(MechPlacementPopup.SetData),
    typeof(SimGameInterruptManager.MechPlacementPopupEntry))]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class MechPlacementShownTrigger
{
    [HarmonyPostfix]
    private static void OnMechPlacementShown(SimGameInterruptManager.MechPlacementPopupEntry entry)
    {
        GameStateExporter.Export(entry.manager.Sim, ExportTrigger.MechPlacementShown);
    }
}
