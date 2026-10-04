using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;
using HBS.Data;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Makes an export file's references to components and collects the definitions of the components referenced,
///     so the file's definitions (e.g. <see cref="GameState.ComponentDefinitions" />) hold exactly those. One
///     instance per export file; it needs only the game's <see cref="DataManager" />, so it also works in combat.
/// </summary>
internal sealed class ComponentDefinitionReader
{
    private readonly DataManager _dataManager;
    private readonly SortedDictionary<string, ComponentDefinition> _definitions = new(StringComparer.Ordinal);

    internal ComponentDefinitionReader(DataManager dataManager)
    {
        _dataManager = dataManager;
    }

    internal IReadOnlyDictionary<string, ComponentDefinition> Definitions => _definitions;

    // Resolves the definition as BaseComponentRef.RefreshComponentDef does.
    internal DefinitionReference ReferenceTo(ComponentType componentType, string componentId) =>
        ReferenceTo(
            componentId,
            componentType switch
            {
                ComponentType.AmmunitionBox => Find(_dataManager.AmmoBoxDefs, componentId),
                ComponentType.HeatSink => Find(_dataManager.HeatSinkDefs, componentId),
                ComponentType.JumpJet => Find(_dataManager.JumpJetDefs, componentId),
                ComponentType.Upgrade => Find(_dataManager.UpgradeDefs, componentId),
                ComponentType.Weapon => Find(_dataManager.WeaponDefs, componentId),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(componentType),
                    componentType,
                    "Not a component definition type")
            });

    // The id stands in for the name of a missing definition, which gets no entry.
    internal DefinitionReference ReferenceTo(string componentId, MechComponentDef? definition)
    {
        if (definition is null)
        {
            return new DefinitionReference(componentId, componentId);
        }

        if (!_definitions.TryGetValue(componentId, out var componentDefinition))
        {
            componentDefinition = ReadDefinition(definition);
            _definitions.Add(componentId, componentDefinition);
        }
        // The game tells definitions apart by type and id; the dictionary's keys assume the ids alone are unique.
        else if (componentDefinition.Type != definition.ComponentType)
        {
            throw new InvalidOperationException(
                $"Component id {componentId} is used by both {componentDefinition.Type} and {definition.ComponentType}");
        }

        return new DefinitionReference(componentId, componentDefinition.Name);
    }

    // The mech lab shows the short UI name (MechLabItemSlotElement).
    private ComponentDefinition ReadDefinition(MechComponentDef definition)
    {
        var description = definition.Description;
        return new ComponentDefinition(
            string.IsNullOrEmpty(description.UIName) ? description.Name : description.UIName,
            definition.ComponentType,
            definition.Tonnage,
            definition.InventorySize,
            description.Cost,
            new[] { definition.BonusValueA, definition.BonusValueB }.Where(bonus => !string.IsNullOrEmpty(bonus))
                .ToList(),
            definition is WeaponDef weapon ? ReadWeapon(weapon) : null,
            // The box's own Ammo is only set once the game has needed it (AmmunitionBoxDef.refreshAmmo).
            definition is AmmunitionBoxDef ammoBox
                ? new AmmoBoxStats(
                    _dataManager.AmmoDefs.Get(ammoBox.AmmoID).AmmoCategoryValue.FriendlyName,
                    ammoBox.Capacity)
                : null,
            definition is HeatSinkDef heatSink ? new HeatSinkStats(heatSink.DissipationCapacity) : null);
    }

    private static MechComponentDef? Find<TDefinition>(IDataItemStore<string, TDefinition> store, string componentId)
        where TDefinition : MechComponentDef, new() =>
        store.Exists(componentId) ? store.Get(componentId) : null;

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
}
