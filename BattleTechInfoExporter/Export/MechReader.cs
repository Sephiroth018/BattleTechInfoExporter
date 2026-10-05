using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
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

    /// <summary>A mech or chassis name followed by the variant, e.g. "Atlas (AS7-D)".</summary>
    /// <remarks>The variant identifies the mech, as in the game's lance and store lists.</remarks>
    private static string NameWithVariant(string name, ChassisDef chassis) => $"{name} ({chassis.VariantName})";

    internal static DefinitionReference ReferenceTo(ChassisDef chassis) =>
        new(chassis.Description.Id, NameWithVariant(chassis.Description.Name, chassis));

    // A mech's name is its nickname (renameable in the mech lab).
    internal static DefinitionReference ReferenceTo(MechDef mech) =>
        new(mech.Description.Id, NameWithVariant(mech.Name, mech.Chassis));

    /// <summary>
    ///     The reference to the mech that mech parts assemble into, or <c>null</c> when its definition is missing.
    /// </summary>
    internal static DefinitionReference? TryReferenceToMech(DataManager dataManager, string mechId) =>
        TryGetMech(dataManager, mechId) is { } mech ? ReferenceTo(mech) : null;

    /// <summary>The mech's definition, or <c>null</c>, with a warning, when it is missing.</summary>
    internal static MechDef? TryGetMech(DataManager dataManager, string mechId)
    {
        if (dataManager.MechDefs.TryGet(mechId, out var mech))
        {
            return mech;
        }

        ModLog.Logger.LogWarning($"Left out {mechId}: no {nameof(BattleTechResourceType.MechDef)} definition");
        return null;
    }

    /// <summary>The chassis' definition, or <c>null</c>, with a warning, when it is missing.</summary>
    internal static ChassisDef? TryGetChassis(DataManager dataManager, string chassisId)
    {
        if (dataManager.ChassisDefs.TryGet(chassisId, out var chassis))
        {
            return chassis;
        }

        ModLog.Logger.LogWarning($"Left out {chassisId}: no {nameof(BattleTechResourceType.ChassisDef)} definition");
        return null;
    }

    internal static MechReference ReferenceToBayMech(MechDef mech) =>
        new(mech.GUID, NameWithVariant(mech.Name, mech.Chassis));

    internal static List<LocationMaxArmor> ReadMaxArmor(ChassisDef chassis) =>
        Locations
            .Select(location =>
            {
                var definition = chassis.GetLocationDef(location);
                return new LocationMaxArmor(
                    location,
                    definition.MaxArmor,
                    HasRearArmor(definition) ? definition.MaxRearArmor : null);
            })
            .ToList();

    /// <summary>The chassis' weapon hardpoints of all locations together.</summary>
    internal static Hardpoints ReadHardpoints(ChassisDef chassis) => ReadHardpoints(chassis, Locations);

    // Summed as TooltipPrefab_Chassis.SetHardpointData does.
    private static Hardpoints ReadHardpoints(ChassisDef chassis, IEnumerable<ChassisLocations> locations)
    {
        int ballistic = 0, energy = 0, missile = 0, support = 0;
        foreach (var location in locations)
        {
            MechStatisticsRules.GetHardpointCountForLocation(
                chassis,
                location,
                ref ballistic,
                ref energy,
                ref missile,
                ref support);
        }

        return new Hardpoints(ballistic, energy, missile, support);
    }

    // Both dictionaries are keyed by the mech bay slot; a slot is in one of them at most.
    internal static List<Mech> ReadMechs(SimGameState simGame, ComponentReferences componentReferences) =>
        simGame.ActiveMechs
            .Concat(simGame.ReadyingMechs)
            .OrderBy(slot => slot.Key)
            .Select(slot => ReadMech(simGame, componentReferences, slot.Key, slot.Value))
            .ToList();

    private static Mech ReadMech(
        SimGameState simGame,
        ComponentReferences componentReferences,
        int slot,
        MechDef mech)
    {
        var chassis = mech.Chassis;
        var workOrder = simGame.GetWorkOrderEntryForMech(mech);
        // A readying mech's work order has no steps: it is the readying itself.
        var refitOrder = workOrder is WorkOrderEntry_ReadyMech ? null : workOrder;
        // MechBayRowGroupWidget.SetData fills each bay row with this many slots.
        var slotsPerBay = simGame.Constants.Story.MaxMechsPerPod;
        return new Mech(
            mech.GUID,
            mech.Name,
            ReferenceTo(chassis),
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
            // SimGameState.GetWorkOrderEntryForMech finds the order among the queue's own entries.
            workOrder is null ? null : simGame.MechLabQueue.IndexOf(workOrder) + 1,
            ReadRefit(simGame, componentReferences, mech, refitOrder),
            MechValidationRules.ValidateMechCanBeFielded(simGame, mech),
            // The mech lab validates at this level, against the mech's work order (MechLabPanel).
            MechValidationRules.ValidateMechDef(MechValidationLevel.MechLab, simGame.DataManager, mech, workOrder)
                .Values
                .SelectMany(problems => problems)
                .Select(problem => problem.ToString())
                .ToList(),
            ReadLoadout(componentReferences, mech),
            refitOrder is null
                ? null
                : ReadLoadout(componentReferences, MechRefit.CopyWithPendingSteps(simGame, mech, refitOrder)));
    }

    // The interrupt queue shows its first entry as curPopup and holds the rest in popups, in display order
    // (SimGameInterruptManager.DisplayIfAvailable). A ChassisDef entry comes only from a deprecated store item type.
    internal static List<MechAwaitingPlacement> ReadMechsAwaitingPlacement(
        SimGameState simGame,
        ComponentReferences componentReferences)
    {
        var interruptQueue = simGame.InterruptQueue;
        return interruptQueue.popups
            .Prepend(interruptQueue.curPopup)
            .OfType<SimGameInterruptManager.MechPlacementPopupEntry>()
            .Select(entry => entry.parameters[0])
            .OfType<MechDef>()
            .Select(mech => new MechAwaitingPlacement(
                ReferenceTo(mech.Chassis),
                mech.Chassis.weightClass,
                mech.Chassis.StockRole,
                ReadLoadout(componentReferences, mech)))
            .ToList();
    }

    private static MechLoadout ReadLoadout(ComponentReferences componentReferences, MechDef mech)
    {
        // The maximum it returns is the stat bar's scale, not the chassis tonnage.
        float usedTonnage = 0, ignoredMax = 0;
        MechStatisticsRules.CalculateTonnage(mech, ref usedTonnage, ref ignoredMax);
        return new MechLoadout(
            new Tonnage(usedTonnage, mech.Chassis.Tonnage),
            // Recomputed by MechDef.RefreshBattleValue whenever the loadout changes; the mech bay shows it.
            mech.Description.Cost,
            MechStatsReader.Read(mech),
            Locations.Select(location => ReadLocation(componentReferences, mech, location)).ToList());
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

    private static MechLocation ReadLocation(
        ComponentReferences componentReferences,
        MechDef mech,
        ChassisLocations location)
    {
        var loadout = mech.GetLocationLoadoutDef(location);
        var definition = mech.GetChassisLocationDef(location);
        var components = mech.Inventory.Where(component => component.MountedLocation == location).ToList();
        return new MechLocation(
            location,
            new Armor(loadout.CurrentArmor, loadout.AssignedArmor, definition.MaxArmor),
            HasRearArmor(definition)
                ? new Armor(loadout.CurrentRearArmor, loadout.AssignedRearArmor, definition.MaxRearArmor)
                : null,
            new Structure(loadout.CurrentInternalStructure, definition.InternalStructure),
            ReadHardpoints(mech.Chassis, [location]),
            // A component whose definition is missing has no known size.
            new Slots(components.Sum(component => component.Def?.InventorySize ?? 0), definition.InventorySlots),
            components.Select(component => new MountedComponent(
                    componentReferences.ReferenceTo(
                        component.ComponentDefType,
                        component.ComponentDefID,
                        component.Def),
                    component.DamageLevel,
                    component.IsFixed))
                .ToList());
    }

    private static List<RefitChange>? ReadRefit(
        SimGameState simGame,
        ComponentReferences componentReferences,
        MechDef mech,
        WorkOrderEntry_MechLab? refitOrder) =>
        refitOrder?.SubEntries
            // SimGameState.UpdateMechLabWorkQueue relies on the same.
            .Cast<WorkOrderEntry_MechLab>()
            .Select(step => ReadRefitChange(simGame, componentReferences, mech, step))
            .ToList();

    private static RefitChange ReadRefitChange(
        SimGameState simGame,
        ComponentReferences componentReferences,
        MechDef mech,
        WorkOrderEntry_MechLab step) =>
        step switch
        {
            WorkOrderEntry_InstallComponent installation => ReadInstallation(componentReferences, installation),
            WorkOrderEntry_RepairComponent repair => ReadRepair(simGame, componentReferences, mech, repair),
            WorkOrderEntry_ModifyMechArmor armor => new RefitChange(
                RefitChangeType.ModifyArmor,
                step.IsMechLabComplete,
                Location: armor.Location,
                FrontArmor: armor.DesiredFrontArmor,
                RearArmor: HasRearArmor(mech.GetChassisLocationDef(armor.Location)) ? armor.DesiredRearArmor : null),
            WorkOrderEntry_RepairMechStructure structure => new RefitChange(
                RefitChangeType.RepairStructure,
                step.IsMechLabComplete,
                Location: structure.Location,
                Structure: structure.StructureAmount),
            _ => throw new InvalidOperationException($"Unexpected mech lab work order type {step.Type}")
        };

    // An install step's MechComponentRef is restored from the save without a DataManager, so its Def stays null; the
    // definition is resolved from the type and id instead. A removal is an install step without a desired location
    // (SimGameState.CreateComponentInstallWorkOrder).
    private static RefitChange ReadInstallation(
        ComponentReferences componentReferences,
        WorkOrderEntry_InstallComponent installation)
    {
        var isRemoval = installation.DesiredLocation == ChassisLocations.None;
        return new RefitChange(
            isRemoval ? RefitChangeType.RemoveComponent : RefitChangeType.InstallComponent,
            installation.IsMechLabComplete,
            componentReferences.ReferenceTo(installation.ComponentType, installation.MechComponentID),
            installation.DamageLevel,
            isRemoval ? installation.PreviousLocation : installation.DesiredLocation);
    }

    // Locations without rear armor have -1 for it.
    private static bool HasRearArmor(LocationDef location) => location.MaxRearArmor >= 0;

    // Finds the component as SimGameState.ML_RepairComponent does: on the mech, among the parts held for the work
    // order, or in storage, where it isn't mounted.
    private static RefitChange ReadRepair(
        SimGameState simGame,
        ComponentReferences componentReferences,
        MechDef mech,
        WorkOrderEntry_RepairComponent repair)
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
            componentReferences.ReferenceTo(repair.ComponentType, repair.MechComponentID, component?.Def),
            repair.DamageLevel,
            component?.MountedLocation is { } location and not ChassisLocations.None ? location : null);
    }
}
