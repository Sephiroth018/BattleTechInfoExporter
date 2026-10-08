using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using BattleTechInfoExporter.Models;
using Mech = BattleTechInfoExporter.Models.Mech;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the mechs of the mech bay from the game's career state.</summary>
internal static class MechReader
{
    // From head to legs, in pairs, as the mech lab lays them out.
    internal static readonly IReadOnlyList<ChassisLocations> Locations =
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

    /// <summary>The chassis name followed by the variant, e.g. "Atlas - AS7-D".</summary>
    /// <remarks>As the mech bay and mech lab show a mech (MechDetails.SetDescriptions), whatever its nickname.</remarks>
    private static string NameWithVariant(ChassisDef chassis) =>
        $"{chassis.Description.Name} - {chassis.VariantName}";

    internal static DefinitionReference ReferenceTo(ChassisDef chassis) =>
        new(chassis.Description.Id, NameWithVariant(chassis));

    internal static DefinitionReference ReferenceTo(MechDef mech) =>
        new(mech.Description.Id, NameWithVariant(mech.Chassis));

    /// <summary>
    ///     The reference to the mech that mech parts assemble into, or <c>null</c> when its definition is missing.
    /// </summary>
    internal static DefinitionReference? TryReferenceToMech(DataManager dataManager, string mechId) =>
        TryGetMech(dataManager, mechId) is { } mech ? ReferenceTo(mech) : null;

    /// <summary>The mech's definition, or <c>null</c>, with a warning, when it is missing.</summary>
    private static MechDef? TryGetMech(DataManager dataManager, string mechId)
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
        new(mech.GUID, NameWithVariant(mech.Chassis));

    // Both dictionaries are keyed by the mech bay slot; a slot is in one of them at most.
    internal static List<Mech> ReadMechs(
        SimGameState simGame,
        IReadOnlyDictionary<WorkOrderEntry, int> mechLabFinishingDays) =>
        simGame.ActiveMechs
            .Concat(simGame.ReadyingMechs)
            .OrderBy(slot => slot.Key)
            .Select(slot => ReadMech(simGame, mechLabFinishingDays, slot.Key, slot.Value))
            .ToList();

    private static Mech ReadMech(
        SimGameState simGame,
        IReadOnlyDictionary<WorkOrderEntry, int> mechLabFinishingDays,
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
            slot / slotsPerBay + 1,
            slot % slotsPerBay + 1,
            workOrder switch
            {
                null => MechStatus.Ready,
                WorkOrderEntry_ReadyMech => MechStatus.Readying,
                _ => MechStatus.InMaintenance
            },
            // SimGameState.GetWorkOrderEntryForMech finds the order among the queue's own entries.
            workOrder is null ? null : mechLabFinishingDays[workOrder],
            workOrder is null ? MechRepair.Estimate(simGame, mech) : null,
            ReadRefit(simGame, mech, refitOrder),
            MechValidationRules.ValidateMechCanBeFielded(simGame, mech),
            // The mech lab validates at this level, against the mech's work order (MechLabPanel).
            MechValidationRules.ValidateMechDef(MechValidationLevel.MechLab, simGame.DataManager, mech, workOrder)
                .Values
                .SelectMany(problems => problems)
                .Select(problem => problem.ToString())
                .ToList(),
            ReadLoadout(mech),
            refitOrder is null
                ? null
                : ReadLoadout(MechRefit.CopyWithPendingSteps(simGame, mech, refitOrder)));
    }

    // The interrupt queue shows its first entry as curPopup and holds the rest in popups, in display order
    // (SimGameInterruptManager.DisplayIfAvailable). A ChassisDef entry comes only from a deprecated store item type.
    internal static List<MechAwaitingPlacement> ReadMechsAwaitingPlacement(SimGameState simGame)
    {
        var interruptQueue = simGame.InterruptQueue;
        return interruptQueue.popups
            .Prepend(interruptQueue.curPopup)
            .OfType<SimGameInterruptManager.MechPlacementPopupEntry>()
            .Select(entry => entry.parameters[0])
            .OfType<MechDef>()
            .Select(mech => new MechAwaitingPlacement(ReferenceTo(mech.Chassis), ReadLoadout(mech)))
            .ToList();
    }

    private static MechLoadout ReadLoadout(MechDef mech) =>
        new(
            ReadUsedTonnage(mech),
            // Recomputed by MechDef.RefreshBattleValue whenever the loadout changes; the mech bay shows it.
            mech.Description.Cost,
            MechStatsReader.Read(mech),
            Locations.Select(location => ReadLocation(mech, location)).ToList());

    /// <summary>The tonnage of the chassis, armor and components.</summary>
    internal static float ReadUsedTonnage(MechDef mech)
    {
        // The maximum it returns is the stat bar's scale, not the chassis tonnage.
        float usedTonnage = 0, ignoredMax = 0;
        MechStatisticsRules.CalculateTonnage(mech, ref usedTonnage, ref ignoredMax);
        return usedTonnage;
    }

    internal static Armor ReadArmor(MechDef mech, ChassisLocations location)
    {
        var loadout = mech.GetLocationLoadoutDef(location);
        return new Armor(loadout.CurrentArmor, loadout.AssignedArmor);
    }

    internal static Armor? ReadRearArmor(MechDef mech, ChassisLocations location)
    {
        var loadout = mech.GetLocationLoadoutDef(location);
        return HasRearArmor(mech.GetChassisLocationDef(location))
            ? new Armor(loadout.CurrentRearArmor, loadout.AssignedRearArmor)
            : null;
    }

    internal static IEnumerable<MechComponentRef> ComponentsMountedIn(MechDef mech, ChassisLocations location) =>
        mech.Inventory.Where(component => component.MountedLocation == location);

    private static MechLocation ReadLocation(MechDef mech, ChassisLocations location)
    {
        return new MechLocation(
            location,
            ReadArmor(mech, location),
            ReadRearArmor(mech, location),
            mech.GetLocationLoadoutDef(location).CurrentInternalStructure,
            ComponentsMountedIn(mech, location)
                .Select(component => new MountedComponent(
                    ComponentReferences.ReferenceTo(component),
                    component.DamageLevel,
                    component.IsFixed))
                .ToList());
    }

    private static List<RefitChange>? ReadRefit(
        SimGameState simGame,
        MechDef mech,
        WorkOrderEntry_MechLab? refitOrder) =>
        refitOrder?.SubEntries
            // SimGameState.UpdateMechLabWorkQueue relies on the same.
            .Cast<WorkOrderEntry_MechLab>()
            .Select(step => ReadRefitChange(simGame, mech, step))
            .ToList();

    private static RefitChange ReadRefitChange(SimGameState simGame, MechDef mech, WorkOrderEntry_MechLab step) =>
        step switch
        {
            WorkOrderEntry_InstallComponent installation => ReadInstallation(simGame.DataManager, installation),
            WorkOrderEntry_RepairComponent repair => ReadRepair(simGame, mech, repair),
            WorkOrderEntry_ModifyMechArmor armor => new ModifyArmorChange(
                step.IsMechLabComplete,
                armor.Location,
                armor.DesiredFrontArmor,
                HasRearArmor(mech.GetChassisLocationDef(armor.Location)) ? armor.DesiredRearArmor : null),
            WorkOrderEntry_RepairMechStructure structure => new RepairStructureChange(
                step.IsMechLabComplete,
                structure.Location,
                structure.StructureAmount),
            _ => throw new InvalidOperationException($"Unexpected mech lab work order type {step.Type}")
        };

    // An install step's MechComponentRef is restored from the save without a DataManager, so its Def stays null; the
    // definition is resolved from the type and id instead. A removal is an install step without a desired location
    // (SimGameState.CreateComponentInstallWorkOrder).
    private static RefitChange ReadInstallation(
        DataManager dataManager,
        WorkOrderEntry_InstallComponent installation)
    {
        var component = ComponentReferences.ReferenceTo(
            dataManager,
            installation.ComponentType,
            installation.MechComponentID);
        return installation.DesiredLocation == ChassisLocations.None
            ? new RemoveComponentChange(
                installation.IsMechLabComplete,
                component,
                installation.DamageLevel,
                installation.PreviousLocation)
            : new InstallComponentChange(
                installation.IsMechLabComplete,
                component,
                installation.DamageLevel,
                installation.DesiredLocation);
    }

    // Locations without rear armor have -1 for it.
    internal static bool HasRearArmor(LocationDef location) => location.MaxRearArmor >= 0;

    // Finds the component as SimGameState.ML_RepairComponent does: on the mech, among the parts held for the work
    // order, or in storage, where it isn't mounted.
    private static RepairComponentChange ReadRepair(
        SimGameState simGame,
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
        return new RepairComponentChange(
            repair.IsMechLabComplete,
            ComponentReferences.ReferenceTo(repair.ComponentType, repair.MechComponentID, component?.Def),
            repair.DamageLevel,
            component?.MountedLocation is { } location and not ChassisLocations.None ? location : null);
    }
}
