using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds the catalog from the definitions the game has loaded: every mech with its chassis and components
///     (SimGameState.RequestDataManagerResources), every other component, and every vehicle, turret and design
///     mask, which <see cref="CatalogExporter" /> loads itself.
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

    internal static Catalog Read(SimGameState simGame, ExportTrigger trigger, string sourceFingerprint)
    {
        var dataManager = simGame.DataManager;
        var combatMultipliers = simGame.CombatConstants.CombatValueMultipliers;
        var mechDefinitions = ToSortedDictionary(dataManager.MechDefs, ReadMech);
        var chassisDefinitions = ToSortedDictionary(
            dataManager.ChassisDefs,
            chassis => ReadChassis(dataManager, chassis));
        var vehicleDefinitions = ToSortedDictionary(
            dataManager.VehicleDefs,
            vehicle => ReadVehicle(combatMultipliers, vehicle));
        var turretDefinitions = ToSortedDictionary(
            dataManager.TurretDefs,
            turret => ReadTurret(combatMultipliers, turret));
        // Also what the catalog's units mount, so every reference in the catalog has its definition.
        var components = ReadComponents(dataManager)
            .Concat(MountedComponents(dataManager.MechDefs, mechDefinitions, mech => mech.Inventory))
            .Concat(MountedComponents(dataManager.VehicleDefs, vehicleDefinitions, vehicle => vehicle.Inventory))
            .Concat(MountedComponents(dataManager.TurretDefs, turretDefinitions, turret => turret.Inventory));

        return new Catalog(
            ModAssembly.Version,
            trigger,
            sourceFingerprint,
            chassisDefinitions,
            mechDefinitions,
            vehicleDefinitions,
            turretDefinitions,
            ReadComponentDefinitions(dataManager, components),
            TerrainReader.ReadTerrainDefinitions(dataManager),
            TerrainReader.ReadBiomeDefinitions(dataManager));
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
                        ReadMaxArmor(definition),
                        definition.InternalStructure,
                        ReadHardpoints(chassis, location),
                        definition.InventorySlots);
                })
                .ToList());

    private static LocationArmor ReadMaxArmor(LocationDef location) =>
        new(location.MaxArmor, MechReader.HasRearArmor(location) ? location.MaxRearArmor : null);

    // Counted as TooltipPrefab_Chassis.SetHardpointData does.
    private static Hardpoints ReadHardpoints(ChassisDef chassis, ChassisLocations location)
    {
        int ballistic = 0, energy = 0, missile = 0, support = 0;
        MechStatisticsRules.GetHardpointCountForLocation(
            chassis,
            location,
            ref ballistic,
            ref energy,
            ref missile,
            ref support);
        return new Hardpoints(ballistic, energy, missile, support);
    }

    // A mech's chassis and fixed components are set once its dependencies are loaded (MechDef.Refresh).
    private static MechDefinition? ReadMech(MechDef mech)
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
                                ComponentReferences.ReferenceTo(component),
                                component.IsFixed))
                            .ToList());
                })
                .ToList());
    }

    // A vehicle's chassis and components are set once its dependencies are loaded (VehicleDef.Refresh). Armor and
    // structure are multiplied as when it spawns (Vehicle.InitStats); its name is the HUD's (Vehicle.Nickname).
    private static VehicleDefinition? ReadVehicle(
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
                        .Select(ComponentReferences.ReferenceTo)
                        .ToList()))
                .ToList());
    }

    // Like a vehicle's (TurretDef.Refresh, Turret.InitStats, Turret.Nickname).
    private static TurretDefinition? ReadTurret(
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
            turret.Inventory.Select(ComponentReferences.ReferenceTo).ToList());
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

    // The components mounted on the units in the catalog, by id; one whose definition is missing has none.
    private static IEnumerable<KeyValuePair<string, MechComponentDef>> MountedComponents<TUnit, TEntry>(
        IEnumerable<KeyValuePair<string, TUnit>> units,
        IReadOnlyDictionary<string, TEntry> entries,
        Func<TUnit, IEnumerable<BaseComponentRef>> readInventory) =>
        units
            .Where(unit => entries.ContainsKey(unit.Key))
            .SelectMany(unit => readInventory(unit.Value))
            .Where(component => component.Def is not null)
            .Select(component => new KeyValuePair<string, MechComponentDef>(component.ComponentDefID, component.Def));

    private static ComponentDefinitions ReadComponentDefinitions(
        DataManager dataManager,
        IEnumerable<KeyValuePair<string, MechComponentDef>> components)
    {
        var weapons = new SortedDictionary<string, WeaponDefinition>(StringComparer.Ordinal);
        var ammunitionBoxes = new SortedDictionary<string, AmmunitionBoxDefinition>(StringComparer.Ordinal);
        var heatSinks = new SortedDictionary<string, HeatSinkDefinition>(StringComparer.Ordinal);
        var jumpJets = new SortedDictionary<string, JumpJetDefinition>(StringComparer.Ordinal);
        var upgrades = new SortedDictionary<string, ComponentDefinition>(StringComparer.Ordinal);
        var readIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var component in components.Where(component => readIds.Add(component.Key)))
        {
            switch (component.Value)
            {
                case WeaponDef weapon:
                    weapons.Add(component.Key, ReadWeapon(weapon));
                    break;
                case AmmunitionBoxDef ammunitionBox:
                    ammunitionBoxes.Add(component.Key, ReadAmmunitionBox(dataManager, ammunitionBox));
                    break;
                case HeatSinkDef heatSink:
                    heatSinks.Add(component.Key, ReadHeatSink(heatSink));
                    break;
                case JumpJetDef jumpJet:
                    jumpJets.Add(component.Key, ReadJumpJet(jumpJet));
                    break;
                case UpgradeDef upgrade:
                    upgrades.Add(component.Key, ReadComponent(upgrade));
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unexpected component definition type {component.Value.GetType().Name} for {component.Key}");
            }
        }

        return new ComponentDefinitions(weapons, ammunitionBoxes, heatSinks, jumpJets, upgrades);
    }

    private static ComponentDefinition ReadComponent(MechComponentDef definition) =>
        new(
            ComponentReferences.NameOf(definition),
            definition.Tonnage,
            definition.InventorySize,
            definition.Description.Cost,
            new[] { definition.BonusValueA, definition.BonusValueB }.Where(bonus => !string.IsNullOrEmpty(bonus))
                .ToList(),
            EffectReader.ReadStatisticChanges(definition.statusEffects),
            // Contract.AddMechComponentToSalvage skips blacklisted components.
            !definition.ComponentTags.Contains(MechValidationRules.Tag_Blacklisted));

    private static WeaponDefinition ReadWeapon(WeaponDef weapon) =>
        new(
            ReadComponent(weapon),
            weapon.WeaponCategoryValue.FriendlyName,
            weapon.AmmoCategoryValue.Is_NotSet || weapon.AmmoCategoryValue.UsesInternalAmmo
                ? null
                : weapon.AmmoCategoryValue.FriendlyName,
            weapon.AmmoCategoryValue.UsesInternalAmmo ? weapon.StartingAmmoCapacity : null,
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

    private static AmmunitionBoxDefinition ReadAmmunitionBox(DataManager dataManager, AmmunitionBoxDef ammunitionBox) =>
        new(ReadComponent(ammunitionBox), ReadAmmoCategory(dataManager, ammunitionBox), ammunitionBox.Capacity);

    // The box's own Ammo is only set once the game has needed it (AmmunitionBoxDef.refreshAmmo). The game guards
    // the lookup as well (AmmunitionBoxDef.GatherDependencies); the ammo's id stands in for a missing definition.
    private static string ReadAmmoCategory(DataManager dataManager, AmmunitionBoxDef ammunitionBox)
    {
        if (dataManager.AmmoDefs.TryGet(ammunitionBox.AmmoID, out var ammo))
        {
            return ammo.AmmoCategoryValue.FriendlyName;
        }

        ModLog.Logger.LogWarning(
            $"Found no ammo {ammunitionBox.AmmoID} of {ammunitionBox.Description.Id}; its id stands in");
        return ammunitionBox.AmmoID;
    }

    private static HeatSinkDefinition ReadHeatSink(HeatSinkDef heatSink) =>
        new(ReadComponent(heatSink), heatSink.DissipationCapacity);

    // JumpCapacity is left out: no game code reads it.
    private static JumpJetDefinition ReadJumpJet(JumpJetDef jumpJet) =>
        new(ReadComponent(jumpJet), jumpJet.MinTonnage, jumpJet.MaxTonnage);
}
