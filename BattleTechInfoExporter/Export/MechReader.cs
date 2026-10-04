using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using UnityEngine;
using Mech = BattleTechInfoExporter.Models.Mech;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the mechs of the mech bay from the game's career state.</summary>
internal static class MechReader
{
    // From head to legs, in pairs, as the mech lab lays them out.
    private static readonly ChassisLocations[] Locations =
    [
        ChassisLocations.Head,
        ChassisLocations.CenterTorso,
        ChassisLocations.LeftTorso,
        ChassisLocations.RightTorso,
        ChassisLocations.LeftArm,
        ChassisLocations.RightArm,
        ChassisLocations.LeftLeg,
        ChassisLocations.RightLeg
    ];

    // Both dictionaries are keyed by the mech bay slot; a slot is in one of them at most.
    internal static List<Mech> ReadMechs(SimGameState simGame) =>
        simGame.ActiveMechs
            .Concat(simGame.ReadyingMechs)
            .OrderBy(slot => slot.Key)
            .Select(slot => ReadMech(simGame, slot.Key, slot.Value))
            .ToList();

    private static Mech ReadMech(SimGameState simGame, int slot, MechDef mech)
    {
        var chassis = mech.Chassis;
        var workOrder = simGame.GetWorkOrderEntryForMech(mech);
        // The maximum it returns is the stat bar's scale, not the chassis tonnage.
        float usedTonnage = 0, ignoredMax = 0;
        MechStatisticsRules.CalculateTonnage(mech, ref usedTonnage, ref ignoredMax);
        // MechBayRowGroupWidget.SetData fills each bay row with this many slots.
        var slotsPerBay = simGame.Constants.Story.MaxMechsPerPod;
        return new Mech(
            mech.GUID,
            mech.Name,
            // E.g. "Atlas AS7-D", like the UI names of the stock mechs.
            new DefinitionReference(chassis.Description.Id, $"{chassis.Description.Name} {chassis.VariantName}"),
            chassis.weightClass,
            chassis.StockRole,
            slot / slotsPerBay + 1,
            slot % slotsPerBay + 1,
            workOrder switch
            {
                null => MechStatus.Ready,
                WorkOrderEntry_ReadyMech => MechStatus.Readying,
                _ => MechStatus.InMaintenance
            },
            ReadDaysUntilReady(simGame, workOrder),
            ReadRefit(simGame, mech, workOrder),
            MechValidationRules.ValidateMechCanBeFielded(simGame, mech),
            // The mech lab validates at this level, against the mech's work order (MechLabPanel).
            MechValidationRules.ValidateMechDef(MechValidationLevel.MechLab, simGame.DataManager, mech, workOrder)
                .Values
                .SelectMany(problems => problems)
                .Select(problem => problem.ToString())
                .ToList(),
            new Tonnage(usedTonnage, chassis.Tonnage),
            // Recomputed by MechDef.RefreshBattleValue whenever the loadout changes; the mech bay shows it.
            mech.Description.Cost,
            MechStatsReader.Read(mech),
            Locations.Select(location => ReadLocation(simGame, mech, location)).ToList());
    }

    // Mirrors TaskTimelineWidget.RefreshEntries and TaskManagementElement.UpdateItem: the mech techs work on
    // the first order of the queue only, so each order waits for the ones before it.
    private static int ReadDaysUntilReady(SimGameState simGame, WorkOrderEntry_MechLab? workOrder)
    {
        if (workOrder is null || workOrder.IsCostPaid())
        {
            return 0;
        }

        var days = 0;
        foreach (var entry in simGame.MechLabQueue.Where(entry => simGame.WorkOrderIsMechTech(entry.Type)))
        {
            if (!entry.IsCostPaid())
            {
                days += Math.Max(1, Mathf.CeilToInt((float)entry.GetRemainingCost() / simGame.MechTechSkill));
            }

            if (entry == workOrder)
            {
                break;
            }
        }

        return days;
    }

    private static MechLocation ReadLocation(SimGameState simGame, MechDef mech, ChassisLocations location)
    {
        var loadout = mech.GetLocationLoadoutDef(location);
        var definition = mech.GetChassisLocationDef(location);
        int ballistic = 0, energy = 0, missile = 0, support = 0;
        MechStatisticsRules.GetHardpointCountForLocation(
            mech,
            location,
            ref ballistic,
            ref energy,
            ref missile,
            ref support);
        var components = mech.Inventory.Where(component => component.MountedLocation == location).ToList();
        return new MechLocation(
            location,
            new Armor(loadout.CurrentArmor, loadout.AssignedArmor, definition.MaxArmor),
            // Locations without rear armor have -1 for it.
            definition.MaxRearArmor < 0
                ? null
                : new Armor(loadout.CurrentRearArmor, loadout.AssignedRearArmor, definition.MaxRearArmor),
            new Structure(loadout.CurrentInternalStructure, definition.InternalStructure),
            new Hardpoints(ballistic, energy, missile, support),
            new Slots(components.Sum(component => component.Def.InventorySize), definition.InventorySlots),
            components.Select(component => ReadEquipment(simGame, component)).ToList());
    }

    private static Equipment ReadEquipment(SimGameState simGame, MechComponentRef component)
    {
        var definition = component.Def;
        var description = definition.Description;
        return new Equipment(
            ReferenceTo(component.ComponentDefID, definition),
            component.ComponentDefType,
            component.DamageLevel,
            component.IsFixed,
            definition.Tonnage,
            definition.InventorySize,
            description.Cost,
            new[] { definition.BonusValueA, definition.BonusValueB }.Where(bonus => !string.IsNullOrEmpty(bonus))
                .ToList(),
            description.Details,
            definition is WeaponDef weapon ? ReadWeapon(weapon) : null,
            // The box's own Ammo is only set once the game has needed it (AmmunitionBoxDef.refreshAmmo).
            definition is AmmunitionBoxDef ammoBox
                ? new AmmoBoxStats(
                    simGame.DataManager.AmmoDefs.Get(ammoBox.AmmoID).AmmoCategoryValue.FriendlyName,
                    ammoBox.Capacity)
                : null,
            definition is HeatSinkDef heatSink ? new HeatSinkStats(heatSink.DissipationCapacity) : null);
    }

    private static WeaponStats ReadWeapon(WeaponDef weapon) =>
        new(
            weapon.WeaponCategoryValue.FriendlyName,
            weapon.AmmoCategoryValue.Is_NotSet || weapon.AmmoCategoryValue.UsesInternalAmmo
                ? null
                : weapon.AmmoCategoryValue.FriendlyName,
            weapon.Damage,
            weapon.Instability,
            weapon.ShotsWhenFired,
            weapon.ProjectilesPerShot,
            weapon.HeatDamage,
            weapon.HeatGenerated,
            new WeaponRanges(
                weapon.MinRange,
                weapon.ShortRange,
                weapon.MediumRange,
                weapon.LongRange,
                weapon.MaxRange),
            weapon.AccuracyModifier,
            weapon.CriticalChanceMultiplier,
            weapon.RefireModifier,
            weapon.IndirectFireCapable);

    // A readying mech's work order has no steps: it is the readying itself.
    private static List<RefitChange>? ReadRefit(
        SimGameState simGame,
        MechDef mech,
        WorkOrderEntry_MechLab? workOrder) =>
        workOrder is null or WorkOrderEntry_ReadyMech
            ? null
            : workOrder.SubEntries
                // SimGameState.UpdateMechLabWorkQueue relies on the same.
                .Cast<WorkOrderEntry_MechLab>()
                .Select(step => ReadRefitChange(simGame, mech, step))
                .ToList();

    private static RefitChange ReadRefitChange(SimGameState simGame, MechDef mech, WorkOrderEntry_MechLab step) =>
        step switch
        {
            // A removal has no desired location (SimGameState.CreateComponentInstallWorkOrder).
            WorkOrderEntry_InstallComponent { DesiredLocation: ChassisLocations.None } removal => new RefitChange(
                RefitChangeType.RemoveComponent,
                step.IsMechLabComplete,
                ReferenceTo(removal.MechComponentID, removal.MechComponentRef.Def),
                removal.DamageLevel,
                removal.PreviousLocation),
            WorkOrderEntry_InstallComponent installation => new RefitChange(
                RefitChangeType.InstallComponent,
                step.IsMechLabComplete,
                ReferenceTo(installation.MechComponentID, installation.MechComponentRef.Def),
                installation.DamageLevel,
                installation.DesiredLocation),
            WorkOrderEntry_RepairComponent repair => ReadRepair(simGame, mech, repair),
            WorkOrderEntry_ModifyMechArmor armor => new RefitChange(
                RefitChangeType.ModifyArmor,
                step.IsMechLabComplete,
                Location: armor.Location,
                FrontArmor: armor.DesiredFrontArmor,
                RearArmor: mech.GetChassisLocationDef(armor.Location).MaxRearArmor < 0
                    ? null
                    : armor.DesiredRearArmor),
            WorkOrderEntry_RepairMechStructure structure => new RefitChange(
                RefitChangeType.RepairStructure,
                step.IsMechLabComplete,
                Location: structure.Location,
                Structure: structure.StructureAmount),
            _ => throw new InvalidOperationException($"Unexpected mech lab work order type {step.Type}")
        };

    // Finds the component as SimGameState.ML_RepairComponent does: on the mech, among the parts held for the work
    // order, or in storage, where it isn't mounted.
    private static RefitChange ReadRepair(SimGameState simGame, MechDef mech, WorkOrderEntry_RepairComponent repair)
    {
        var isFromStorage = false;
        var component = simGame.GetMechComponentRefForUID(
            mech,
            repair.ComponentSimGameUID,
            repair.MechComponentID,
            repair.ComponentType,
            repair.DamageLevel,
            ChassisLocations.None,
            -1,
            ref isFromStorage);
        return new RefitChange(
            RefitChangeType.RepairComponent,
            repair.IsMechLabComplete,
            ReferenceTo(repair.MechComponentID, component?.Def),
            repair.DamageLevel,
            component?.MountedLocation is { } location and not ChassisLocations.None ? location : null);
    }

    // The mech lab shows the short UI name (MechLabItemSlotElement); the id stands in for a missing definition.
    private static DefinitionReference ReferenceTo(string componentId, MechComponentDef? definition) =>
        new(
            componentId,
            definition is null ? componentId
            : string.IsNullOrEmpty(definition.Description.UIName) ? definition.Description.Name
            : definition.Description.UIName);
}
