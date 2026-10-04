using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Makes an export file's references to components and collects the definitions of the components referenced,
///     so the file's <see cref="ComponentDefinitions" /> hold exactly those. One instance per export file; it needs
///     only the game's <see cref="DataManager" />, so it also works in combat.
/// </summary>
internal sealed class ComponentDefinitionReader
{
    private readonly SortedDictionary<string, AmmunitionBoxDefinition> _ammunitionBoxes = new(StringComparer.Ordinal);
    private readonly DataManager _dataManager;
    private readonly SortedDictionary<string, HeatSinkDefinition> _heatSinks = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, ComponentDefinition> _jumpJets = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, ComponentDefinition> _upgrades = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, WeaponDefinition> _weapons = new(StringComparer.Ordinal);

    internal ComponentDefinitionReader(DataManager dataManager)
    {
        _dataManager = dataManager;
    }

    internal ComponentDefinitions Definitions => new(_weapons, _ammunitionBoxes, _heatSinks, _jumpJets, _upgrades);

    // DataManager.Get returns null for a missing definition; the type mapping is static, so no SimGameState is needed.
    internal ComponentReference ReferenceTo(ComponentType componentType, string componentId) =>
        ReferenceTo(
            componentType,
            componentId,
            _dataManager.Get(SimGameState.ComponentTypeToBattleTechResourceType(componentType), componentId)
                as MechComponentDef);

    // The id stands in for the name of a missing definition, which gets no entry.
    internal ComponentReference ReferenceTo(
        ComponentType componentType,
        string componentId,
        MechComponentDef? definition)
    {
        if (definition is null)
        {
            return new ComponentReference(componentId, componentId, componentType);
        }

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

        // The definition's own type, so the reference names the group that holds it.
        return new ComponentReference(componentId, NameOf(definition), definition.ComponentType);
    }

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
                .ToList());

    private static WeaponDefinition ReadWeapon(WeaponDef weapon)
    {
        var component = ReadComponent(weapon);
        return new WeaponDefinition(
            component.Name,
            component.Tonnage,
            component.Slots,
            component.Cost,
            component.Bonuses,
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
    }

    private AmmunitionBoxDefinition ReadAmmunitionBox(AmmunitionBoxDef ammunitionBox)
    {
        var component = ReadComponent(ammunitionBox);
        return new AmmunitionBoxDefinition(
            component.Name,
            component.Tonnage,
            component.Slots,
            component.Cost,
            component.Bonuses,
            // The box's own Ammo is only set once the game has needed it (AmmunitionBoxDef.refreshAmmo).
            _dataManager.AmmoDefs.Get(ammunitionBox.AmmoID).AmmoCategoryValue.FriendlyName,
            ammunitionBox.Capacity);
    }

    private static HeatSinkDefinition ReadHeatSink(HeatSinkDef heatSink)
    {
        var component = ReadComponent(heatSink);
        return new HeatSinkDefinition(
            component.Name,
            component.Tonnage,
            component.Slots,
            component.Cost,
            component.Bonuses,
            heatSink.DissipationCapacity);
    }
}
