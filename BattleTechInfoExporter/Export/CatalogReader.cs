using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds the catalog from the definitions the game has loaded for the career: every mech with its chassis and
///     components (SimGameState.RequestDataManagerResources), and every other component.
/// </summary>
internal static class CatalogReader
{
    // The tutorial's target dummies, which can never be owned: e.g. chassisdef_panther_TARGETDUMMY,
    // mechdef_urbanmech_TESTDUMMY on a real chassis, and their TargetDummyMod upgrade.
    private const string DummyIdPart = "DUMMY";

    internal static Catalog Read(SimGameState simGame, ExportTrigger trigger)
    {
        var dataManager = simGame.DataManager;
        var componentReferences = new ComponentReferences(dataManager);
        var mechDefinitions = ToSortedDictionary(
            dataManager.MechDefs,
            mech => ReadMech(componentReferences, mech));
        var chassisDefinitions = ToSortedDictionary(
            dataManager.ChassisDefs,
            chassis => ReadChassis(dataManager, chassis));
        foreach (var component in ReadComponents(dataManager))
        {
            componentReferences.AddDefinition(component.Key, component.Value);
        }

        return new Catalog(
            ModAssembly.Version,
            trigger,
            chassisDefinitions,
            mechDefinitions,
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
            if (!IsDummy(definition.Key) && readEntry(definition.Value) is { } entry)
            {
                entries.Add(definition.Key, entry);
            }
        }

        return entries;
    }

    private static bool IsDummy(string id) => id.IndexOf(DummyIdPart, StringComparison.Ordinal) >= 0;

    private static ChassisDefinition ReadChassis(DataManager dataManager, ChassisDef chassis) =>
        new(
            MechReader.ReferenceTo(chassis).Name,
            chassis.weightClass,
            chassis.Tonnage,
            chassis.InitialTonnage,
            chassis.MaxJumpjets,
            chassis.Heatsinks,
            new ChassisMovement(chassis.MovementCapDef.MaxWalkDistance, chassis.MovementCapDef.MaxSprintDistance),
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
            .Where(component => !IsDummy(component.Key));

    private static KeyValuePair<string, MechComponentDef> AsComponent<TDefinition>(
        KeyValuePair<string, TDefinition> definition)
        where TDefinition : MechComponentDef =>
        new(definition.Key, definition.Value);
}
