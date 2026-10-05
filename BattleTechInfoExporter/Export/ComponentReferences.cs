using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Makes an export file's references to components and collects the definitions of the components referenced,
///     so the file's <see cref="ComponentDefinitions" /> hold exactly those, apart from definitions added on their
///     own (<see cref="AddDefinition" />). One instance per export file; it needs
///     only the game's <see cref="DataManager" />, so it also works in combat.
/// </summary>
internal sealed class ComponentReferences
{
    private readonly SortedDictionary<string, AmmunitionBoxDefinition> _ammunitionBoxes = new(StringComparer.Ordinal);
    private readonly DataManager _dataManager;
    private readonly SortedDictionary<string, HeatSinkDefinition> _heatSinks = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, ComponentDefinition> _jumpJets = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, ComponentDefinition> _upgrades = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, WeaponDefinition> _weapons = new(StringComparer.Ordinal);

    internal ComponentReferences(DataManager dataManager)
    {
        _dataManager = dataManager;
    }

    internal ComponentDefinitions Definitions => new(_weapons, _ammunitionBoxes, _heatSinks, _jumpJets, _upgrades);

    // The type mapping is static, so no SimGameState is needed.
    internal ComponentReference ReferenceTo(ComponentType componentType, string componentId) =>
        ReferenceTo(
            componentType,
            componentId,
            FindDefinition(SimGameState.ComponentTypeToBattleTechResourceType(componentType), componentId));

    internal ComponentReference ReferenceTo(BaseComponentRef component) =>
        ReferenceTo(component.ComponentDefType, component.ComponentDefID, component.Def);

    internal ComponentReference? TryReferenceTo(ComponentType componentType, string componentId) =>
        TryReferenceTo(SimGameState.ComponentTypeToBattleTechResourceType(componentType), componentId);

    /// <summary>The reference to a component, or <c>null</c> when its definition is missing.</summary>
    /// <remarks>For lists that leave out what they can't describe, unlike the other overloads.</remarks>
    internal ComponentReference? TryReferenceTo(BattleTechResourceType resourceType, string componentId)
    {
        if (FindDefinition(resourceType, componentId) is { } definition)
        {
            return ReferenceTo(definition.ComponentType, componentId, definition);
        }

        ModLog.Logger.LogWarning($"Left out {componentId}: no {resourceType} definition");
        return null;
    }

    // The id stands in for the name of a missing definition, which gets no entry.
    internal ComponentReference ReferenceTo(
        ComponentType componentType,
        string componentId,
        MechComponentDef? definition)
    {
        if (definition is null)
        {
            ModLog.Logger.LogWarning($"Found no definition of {componentType} {componentId}; its id stands in");
            return new ComponentReference(componentId, componentId, componentType);
        }

        AddDefinition(componentId, definition);
        // The definition's own type, so the reference names the group that holds it.
        return new ComponentReference(componentId, NameOf(definition), definition.ComponentType);
    }

    /// <summary>Collects a definition whether or not anything in the file refers to it.</summary>
    internal void AddDefinition(string componentId, MechComponentDef definition)
    {
        switch (definition)
        {
            case WeaponDef weapon:
                AddOnce(_weapons, componentId, () => ReadWeapon(weapon));
                break;
            case AmmunitionBoxDef ammunitionBox:
                AddOnce(_ammunitionBoxes, componentId, () => ReadAmmunitionBox(ammunitionBox));
                break;
            case HeatSinkDef heatSink:
                AddOnce(_heatSinks, componentId, () => ReadHeatSink(heatSink));
                break;
            case JumpJetDef:
                AddOnce(_jumpJets, componentId, () => ReadComponent(definition));
                break;
            case UpgradeDef:
                AddOnce(_upgrades, componentId, () => ReadComponent(definition));
                break;
            default:
                throw new InvalidOperationException(
                    $"Unexpected component definition type {definition.GetType().Name} for {componentId}");
        }
    }

    // DataManager.Get returns null for a missing definition.
    private MechComponentDef? FindDefinition(BattleTechResourceType resourceType, string componentId) =>
        _dataManager.Get(resourceType, componentId) as MechComponentDef;

    private static void AddOnce<TDefinition>(
        SortedDictionary<string, TDefinition> definitions,
        string componentId,
        Func<TDefinition> readDefinition)
    {
        if (!definitions.ContainsKey(componentId))
        {
            definitions.Add(componentId, readDefinition());
        }
    }

    // The mech lab shows the short UI name (MechLabItemSlotElement).
    private static string NameOf(MechComponentDef definition) =>
        string.IsNullOrEmpty(definition.Description.UIName)
            ? definition.Description.Name
            : definition.Description.UIName;

    private static ComponentDefinition ReadComponent(MechComponentDef definition) =>
        new(
            NameOf(definition),
            definition.Tonnage,
            definition.InventorySize,
            definition.Description.Cost,
            new[] { definition.BonusValueA, definition.BonusValueB }.Where(bonus => !string.IsNullOrEmpty(bonus))
                .ToList(),
            // Contract.AddMechComponentToSalvage skips blacklisted components.
            !definition.ComponentTags.Contains(MechValidationRules.Tag_Blacklisted));

    private static WeaponDefinition ReadWeapon(WeaponDef weapon) =>
        new(
            ReadComponent(weapon),
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

    private AmmunitionBoxDefinition ReadAmmunitionBox(AmmunitionBoxDef ammunitionBox) =>
        new(ReadComponent(ammunitionBox), ReadAmmoCategory(ammunitionBox), ammunitionBox.Capacity);

    // The box's own Ammo is only set once the game has needed it (AmmunitionBoxDef.refreshAmmo). The game guards
    // the lookup as well (AmmunitionBoxDef.GatherDependencies); the ammo's id stands in for a missing definition.
    private string ReadAmmoCategory(AmmunitionBoxDef ammunitionBox)
    {
        if (_dataManager.AmmoDefs.TryGet(ammunitionBox.AmmoID, out var ammo))
        {
            return ammo.AmmoCategoryValue.FriendlyName;
        }

        ModLog.Logger.LogWarning(
            $"Found no ammo {ammunitionBox.AmmoID} of {ammunitionBox.Description.Id}; its id stands in");
        return ammunitionBox.AmmoID;
    }

    private static HeatSinkDefinition ReadHeatSink(HeatSinkDef heatSink) =>
        new(ReadComponent(heatSink), heatSink.DissipationCapacity);
}
