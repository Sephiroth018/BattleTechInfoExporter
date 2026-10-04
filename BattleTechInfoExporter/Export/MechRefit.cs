using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds the loadout a refit leads to, as the mech lab shows it (MechLabPanel.ApplyWorkOrder): the unfinished
///     steps are applied to a copy of the mech the way SimGameState's ML_ methods apply each one when it finishes.
/// </summary>
/// <remarks>
///     Components found in the game's state are copied before they change, so the game's own mech and work order
///     components stay as they are.
/// </remarks>
internal sealed class MechRefit
{
    private readonly MechDef _copy;

    // Components taken off the copy or found elsewhere in the game, by their SimGameUID.
    private readonly Dictionary<string, MechComponentRef> _heldComponents = new();
    private readonly List<MechComponentRef> _inventory;
    private readonly SimGameState _simGame;

    private MechRefit(SimGameState simGame, MechDef copy)
    {
        _simGame = simGame;
        _copy = copy;
        _inventory = copy.Inventory.ToList();
    }

    internal static MechDef CopyWithPendingSteps(
        SimGameState simGame,
        MechDef mech,
        WorkOrderEntry_MechLab refitOrder)
    {
        var refit = new MechRefit(simGame, new MechDef(mech));
        // SimGameState.UpdateMechLabWorkQueue relies on the same cast.
        foreach (var step in refitOrder.SubEntries.Cast<WorkOrderEntry_MechLab>()
                     .Where(step => !step.IsMechLabComplete))
        {
            refit.Apply(step);
        }

        return refit.Finish();
    }

    private void Apply(WorkOrderEntry_MechLab step)
    {
        switch (step)
        {
            case WorkOrderEntry_InstallComponent installation:
                Install(installation);
                break;
            case WorkOrderEntry_RepairComponent repair:
                Repair(repair);
                break;
            case WorkOrderEntry_ModifyMechArmor armor:
                // Mirrors SimGameState.ML_ModifyArmor.
                var armorLoadout = _copy.GetLocationLoadoutDef(armor.Location);
                armorLoadout.CurrentArmor = armor.DesiredFrontArmor;
                armorLoadout.CurrentRearArmor = armor.DesiredRearArmor;
                armorLoadout.AssignedArmor = armor.DesiredFrontArmor;
                armorLoadout.AssignedRearArmor = armor.DesiredRearArmor;
                break;
            case WorkOrderEntry_RepairMechStructure structure:
                // Mirrors SimGameState.ML_RepairMech.
                var structureLoadout = _copy.GetLocationLoadoutDef(structure.Location);
                structureLoadout.CurrentInternalStructure =
                    _copy.GetChassisLocationDef(structure.Location).InternalStructure;
                structureLoadout.CurrentArmor = structureLoadout.AssignedArmor;
                structureLoadout.CurrentRearArmor = structureLoadout.AssignedRearArmor;
                break;
            default:
                throw new InvalidOperationException($"Unexpected mech lab work order type {step.Type}");
        }
    }

    // Mirrors SimGameState.ML_InstallComponent, which also handles removals: they have no desired location.
    private void Install(WorkOrderEntry_InstallComponent installation)
    {
        var component = FindComponent(
            installation.ComponentSimGameUID,
            installation.MechComponentID,
            installation.ComponentType,
            installation.DamageLevel);
        if (component is null)
        {
            return;
        }

        _inventory.Remove(component);
        if (installation.DesiredLocation == ChassisLocations.None)
        {
            if (component.DamageLevel != ComponentDamageLevel.Destroyed)
            {
                component.SetData(
                    ChassisLocations.None,
                    installation.HardpointSlot,
                    component.DamageLevel switch
                    {
                        ComponentDamageLevel.Functional => ComponentDamageLevel.Installing,
                        ComponentDamageLevel.NonFunctional => ComponentDamageLevel.InstallingNonFunctional,
                        _ => ComponentDamageLevel.Functional
                    },
                    component.IsFixed);
            }

            _heldComponents[component.SimGameUID] = component;
        }
        else
        {
            component.SetData(
                installation.DesiredLocation,
                installation.HardpointSlot,
                component.DamageLevel == ComponentDamageLevel.InstallingNonFunctional
                    ? ComponentDamageLevel.NonFunctional
                    : ComponentDamageLevel.Functional,
                component.IsFixed);
            _heldComponents.Remove(component.SimGameUID);
            _inventory.Add(component);
        }
    }

    // Mirrors SimGameState.ML_RepairComponent for a step of a mech's work order.
    private void Repair(WorkOrderEntry_RepairComponent repair)
    {
        var component = FindComponent(
            repair.ComponentSimGameUID,
            repair.MechComponentID,
            repair.ComponentType,
            repair.DamageLevel);
        if (component is not null)
        {
            component.DamageLevel = ComponentDamageLevel.Functional;
        }
    }

    // Looks where SimGameState.GetMechComponentRefForUID does, with the copy and the components taken off it in
    // place of the game's mech.
    private MechComponentRef? FindComponent(
        string simGameUid,
        string componentId,
        ComponentType componentType,
        ComponentDamageLevel damageLevel)
    {
        var component = _inventory.Find(candidate => candidate.SimGameUID == simGameUid);
        if (component is not null || _heldComponents.TryGetValue(simGameUid, out component))
        {
            return component;
        }

        var isFromStorage = false;
        var gameComponent = _simGame.GetMechComponentRefForUID(
            null,
            simGameUid,
            componentId,
            componentType,
            damageLevel,
            ChassisLocations.None,
            -1,
            ref isFromStorage);
        if (gameComponent is null)
        {
            // SimGameState's ML_ methods skip the step as well.
            ModLog.Logger.LogWarning(
                $"Skipped a refit step of mech {_copy.GUID}: found no component {componentId} ({simGameUid})");
            return null;
        }

        component = new MechComponentRef(gameComponent);
        _heldComponents[simGameUid] = component;
        return component;
    }

    // MechDef.SetInventory resolves the components' definitions, which their copies don't carry.
    private MechDef Finish()
    {
        _copy.SetInventory(_inventory.ToArray());
        _copy.RefreshBattleValue();
        return _copy;
    }
}
