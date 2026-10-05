using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds the catalog from the definitions the game has loaded: every mech with its chassis and components
///     (SimGameState.RequestDataManagerResources), every other component, and every vehicle and turret, which
///     <see cref="CatalogExporter" /> loads itself.
/// </summary>
internal static class CatalogReader
{
    // The tutorial's target dummies, which can never be owned: e.g. chassisdef_panther_TARGETDUMMY,
    // mechdef_urbanmech_TESTDUMMY on a real chassis, and their TargetDummyMod upgrade.
    private const string DummyIdPart = "DUMMY";

    // The tutorial's target vehicles, vehicledef_TARGETVEHICLE1 and 2, named "Target Dummy".
    private const string TargetVehicleIdPart = "TARGETVEHICLE";

    // The order of VehicleDef's locations, with the turret last because not every vehicle has one.
    private static readonly IReadOnlyList<VehicleChassisLocations> VehicleLocations =
    [
        VehicleChassisLocations.Front,
        VehicleChassisLocations.Left,
        VehicleChassisLocations.Right,
        VehicleChassisLocations.Rear,
        VehicleChassisLocations.Turret
    ];

    internal static Catalog Read(SimGameState simGame, ExportTrigger trigger)
    {
        var dataManager = simGame.DataManager;
        var combatMultipliers = simGame.CombatConstants.CombatValueMultipliers;
        var componentReferences = new ComponentReferences(dataManager);
        var mechDefinitions = ToSortedDictionary(
            dataManager.MechDefs,
            mech => ReadMech(componentReferences, mech));
        var chassisDefinitions = ToSortedDictionary(
            dataManager.ChassisDefs,
            chassis => ReadChassis(dataManager, chassis));
        var vehicleDefinitions = ToSortedDictionary(
            dataManager.VehicleDefs,
            vehicle => ReadVehicle(componentReferences, combatMultipliers, vehicle));
        var turretDefinitions = ToSortedDictionary(
            dataManager.TurretDefs,
            turret => ReadTurret(componentReferences, combatMultipliers, turret));
        foreach (var component in ReadComponents(dataManager))
        {
            componentReferences.AddDefinition(component.Key, component.Value);
        }

        return new Catalog(
            ModAssembly.Version,
            trigger,
            chassisDefinitions,
            mechDefinitions,
            vehicleDefinitions,
            turretDefinitions,
            componentReferences.Definitions);
    }

    private static SortedDictionary<string, TEntry> ToSortedDictionary<TDefinition, TEntry>(
        IEnumerable<KeyValuePair<string, TDefinition>> definitions,
        Func<TDefinition, TEntry?> readEntry)
        where TEntry : class
    {
        var entries = new SortedDictionary<string, TEntry>(StringComparer.Ordinal);
        foreach (var definition in definitions)
        {
            if (!IsLeftOut(definition.Key) && readEntry(definition.Value) is { } entry)
            {
                entries.Add(definition.Key, entry);
            }
        }

        return entries;
    }

    // Also the copies of stock mechs AssetUnlocks.CreateCustomMechsForBase makes for unlocked skins, e.g.
    // UNLOCKED_chrPrfMech_shadowhawkBacker-UMBRA-SHD-2D: only their look differs.
    private static bool IsLeftOut(string id) =>
        id.IndexOf(DummyIdPart, StringComparison.Ordinal) >= 0
        || id.IndexOf(TargetVehicleIdPart, StringComparison.Ordinal) >= 0
        || id.StartsWith(AssetUnlocks.UnlockedIdPrefix, StringComparison.Ordinal);

    private static ChassisDefinition ReadChassis(DataManager dataManager, ChassisDef chassis) =>
        new(
            MechReader.ReferenceTo(chassis).Name,
            chassis.weightClass,
            chassis.Tonnage,
            chassis.InitialTonnage,
            chassis.MaxJumpjets,
            chassis.Heatsinks,
            ReadMovement(chassis.MovementCapDef),
            MechStatsReader.ReadChassisMelee(chassis),
            // The mech lab's stock popup, the store and mech assembly find the stock mech by this id
            // (MechLabStockInfoPopup, SG_Shop_Screen, SimGameState).
            MechReader.TryReferenceToMech(
                dataManager,
                chassis.Description.Id.Replace("chassisdef", "mechdef")),
            MechReader.Locations
                .Select(location =>
                {
                    var definition = chassis.GetLocationDef(location);
                    return new ChassisLocationDefinition(
                        location,
                        MechReader.ReadMaxArmor(definition),
                        definition.InternalStructure,
                        MechReader.ReadHardpoints(chassis, [location]));
                })
                .ToList());

    // A mech's chassis and fixed components are set once its dependencies are loaded (MechDef.Refresh).
    private static MechDefinition? ReadMech(ComponentReferences componentReferences, MechDef mech)
    {
        if (mech.Chassis is null)
        {
            ModLog.Logger.LogWarning($"Left out {mech.Description.Id}: its chassis {mech.ChassisID} isn't loaded");
            return null;
        }

        return new MechDefinition(
            MechReader.ReferenceTo(mech).Name,
            MechReader.ReferenceTo(mech.Chassis),
            mech.Description.Cost,
            MechReader.ReadUsedTonnage(mech),
            MechStatsReader.Read(mech),
            // Contract.CreateAndAddMechPart gives no mech parts of a blacklisted mech.
            !mech.MechTags.Contains(MechValidationRules.Tag_Blacklisted),
            MechReader.Locations
                .Select(location =>
                {
                    var loadout = mech.GetLocationLoadoutDef(location);
                    return new MechLocationDefinition(
                        location,
                        new LocationArmor(
                            loadout.AssignedArmor,
                            MechReader.HasRearArmor(mech.GetChassisLocationDef(location))
                                ? loadout.AssignedRearArmor
                                : null),
                        MechReader.ComponentsMountedIn(mech, location)
                            .Select(component => new LoadoutComponent(
                                componentReferences.ReferenceTo(component),
                                component.IsFixed))
                            .ToList());
                })
                .ToList());
    }

    // A vehicle's chassis and components are set once its dependencies are loaded (VehicleDef.Refresh). Armor and
    // structure are multiplied as when it spawns (Vehicle.InitStats); its name is the HUD's (Vehicle.Nickname).
    private static VehicleDefinition? ReadVehicle(
        ComponentReferences componentReferences,
        CombatValueMultipliersDef combatMultipliers,
        VehicleDef vehicle)
    {
        if (vehicle.Chassis is not { } chassis)
        {
            ModLog.Logger.LogWarning(
                $"Left out {vehicle.Description.Id}: its chassis {vehicle.ChassisID} isn't loaded");
            return null;
        }

        return new VehicleDefinition(
            vehicle.Description.Name,
            chassis.weightClass,
            chassis.Tonnage,
            chassis.movementType,
            ReadMovement(chassis.MovementCapDef),
            VehicleLocations
                .Where(location => location != VehicleChassisLocations.Turret || chassis.HasTurret)
                .Select(location => new VehicleLocationDefinition(
                    location,
                    vehicle.GetLocationLoadoutDef(location).AssignedArmor * combatMultipliers.ArmorMultiplierVehicle,
                    vehicle.GetChassisLocationDef(location).InternalStructure
                    * combatMultipliers.StructureMultiplierVehicle,
                    vehicle.Inventory
                        .Where(component => component.MountedLocation == location)
                        .Select(componentReferences.ReferenceTo)
                        .ToList()))
                .ToList());
    }

    // Like a vehicle's (TurretDef.Refresh, Turret.InitStats, Turret.Nickname).
    private static TurretDefinition? ReadTurret(
        ComponentReferences componentReferences,
        CombatValueMultipliersDef combatMultipliers,
        TurretDef turret)
    {
        if (turret.Chassis is not { } chassis)
        {
            ModLog.Logger.LogWarning($"Left out {turret.Description.Id}: its chassis {turret.ChassisID} isn't loaded");
            return null;
        }

        return new TurretDefinition(
            turret.Description.Name,
            chassis.weightClass,
            chassis.Tonnage,
            chassis.FiringArcDegrees,
            turret.AssignedArmor * combatMultipliers.ArmorMultiplierVehicle,
            chassis.MaxInternalStructure * combatMultipliers.StructureMultiplierVehicle,
            turret.Inventory.Select(componentReferences.ReferenceTo).ToList());
    }

    private static ChassisMovement ReadMovement(MovementCapabilitiesDef movement) =>
        new(movement.MaxWalkDistance, movement.MaxSprintDistance);

    // Every component type, keyed by id, without the dummies' and the game's internal weapons: the melee and jump
    // attacks and the AI's imaginary laser, which aren't mounted (MechDef.CreateMeleeWeaponRefs).
    private static IEnumerable<KeyValuePair<string, MechComponentDef>> ReadComponents(DataManager dataManager) =>
        dataManager.WeaponDefs
            .Where(weapon => weapon.Value.WeaponSubType is not (WeaponSubType.Melee or WeaponSubType.DFA
                or WeaponSubType.AIImaginary))
            .Select(AsComponent)
            .Concat(dataManager.AmmoBoxDefs.Select(AsComponent))
            .Concat(dataManager.HeatSinkDefs.Select(AsComponent))
            .Concat(dataManager.JumpJetDefs.Select(AsComponent))
            .Concat(dataManager.UpgradeDefs.Select(AsComponent))
            .Where(component => !IsLeftOut(component.Key));

    private static KeyValuePair<string, MechComponentDef> AsComponent<TDefinition>(
        KeyValuePair<string, TDefinition> definition)
        where TDefinition : MechComponentDef =>
        new(definition.Key, definition.Value);
}
