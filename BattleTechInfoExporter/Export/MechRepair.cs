using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using UnityEngine;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Estimates the mech bay's Repair of a mech as its confirmation prices it (MechBayPanel.OnRepairMech): the
///     structure of every damaged location, the damaged components and the removal of the destroyed ones.
/// </summary>
/// <remarks>
///     The prices are computed here instead of through SimGameState's Create…WorkOrder methods, because those
///     generate ids and so change the game's state.
/// </remarks>
internal static class MechRepair
{
    /// <summary>The estimate for a mech without a work order, or <c>null</c> when it has nothing to repair.</summary>
    internal static RepairEstimate? Estimate(SimGameState simGame, MechDef mech)
    {
        var constants = simGame.Constants.MechLab;
        var prices = MechReader.Locations
            .Where(mech.IsLocationDamaged)
            .Select(location => PriceStructureRepair(constants, mech, location))
            .Concat(mech.Inventory
                .Where(component =>
                    component.DamageLevel is not (ComponentDamageLevel.Functional or ComponentDamageLevel.Installing))
                .Select(component => PriceComponentRepair(constants, component)))
            .ToList();
        return prices.Count == 0
            ? null
            : new RepairEstimate(
                WorkQueueReader.DaysFor(prices.Sum(price => price.TechPoints), simGame.MechTechSkill),
                prices.Sum(price => price.CBills));
    }

    // SimGameState.CreateMechRepairWorkOrder, for the missing structure as MechBayPanel.OnRepairMech rounds it.
    private static (int TechPoints, int CBills) PriceStructureRepair(
        MechLabConstantsDef constants,
        MechDef mech,
        ChassisLocations location)
    {
        var maxStructure = mech.GetChassisLocationDef(location).InternalStructure;
        var missingStructure = Mathf.RoundToInt(
            Mathf.Max(0f, maxStructure - mech.GetLocationLoadoutDef(location).CurrentInternalStructure));
        // A destroyed location's structure costs differently.
        var isDestroyed = Mathf.Approximately(maxStructure, missingStructure);
        var techPointModifier = isDestroyed ? constants.ZeroStructureTechPointModifier : 1f;
        var cBillModifier = isDestroyed ? constants.ZeroStructureCBillModifier : 1f;
        return (Mathf.CeilToInt(constants.StructureRepairTechPoints * missingStructure * techPointModifier),
            Mathf.CeilToInt(constants.StructureRepairCost * missingStructure * cBillModifier));
    }

    // A damaged component is repaired (SimGameState.CreateComponentRepairWorkOrder), a destroyed one removed without
    // C-Bills (SimGameState.CreateComponentInstallWorkOrder).
    private static (int TechPoints, int CBills) PriceComponentRepair(
        MechLabConstantsDef constants,
        MechComponentRef component)
    {
        // A component whose definition is missing has no known size.
        var size = component.Def?.InventorySize ?? 0;
        if (component.DamageLevel == ComponentDamageLevel.Destroyed)
        {
            return (constants.UninstallTechPoints * size, 0);
        }

        return component.IsFixed
            ? (0, 0)
            : (constants.ComponentRepairTechPoints * size, constants.ComponentRepairCost * size);
    }
}
