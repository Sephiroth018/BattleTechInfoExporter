using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     One step of a mech's queued refit; each value of <see cref="Type" /> has a record with the fields of its own.
/// </summary>
/// <param name="Type">The kind of step, which decides the step's other fields.</param>
/// <param name="IsDone">Whether the mech techs have finished the step; its change is then part of the mech.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record RefitChange(RefitChangeType Type, bool IsDone);

/// <summary>A step that installs, removes or repairs a component.</summary>
/// <param name="Component">The component the step installs, removes or repairs.</param>
/// <param name="DamageLevel">The component's damage level when the step was ordered.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record ComponentRefitChange(
    RefitChangeType Type,
    bool IsDone,
    ComponentReference Component,
    ComponentDamageLevel DamageLevel) : RefitChange(Type, IsDone), IDamageable;

/// <summary>A step that installs a component.</summary>
/// <param name="Location">Where the component goes.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record InstallComponentChange(
    bool IsDone,
    ComponentReference Component,
    ComponentDamageLevel DamageLevel,
    ChassisLocations Location)
    : ComponentRefitChange(RefitChangeType.InstallComponent, IsDone, Component, DamageLevel);

/// <summary>A step that removes a component.</summary>
/// <param name="Location">Where the component comes from.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RemoveComponentChange(
    bool IsDone,
    ComponentReference Component,
    ComponentDamageLevel DamageLevel,
    ChassisLocations Location)
    : ComponentRefitChange(RefitChangeType.RemoveComponent, IsDone, Component, DamageLevel);

/// <summary>A step that repairs a component.</summary>
/// <param name="Location">Where the component is mounted; <c>null</c> for one in storage.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RepairComponentChange(
    bool IsDone,
    ComponentReference Component,
    ComponentDamageLevel DamageLevel,
    ChassisLocations? Location)
    : ComponentRefitChange(RefitChangeType.RepairComponent, IsDone, Component, DamageLevel);

/// <summary>A step that changes a location's armor or structure.</summary>
/// <param name="Location">The mech location whose armor or structure the step changes.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record LocationRefitChange(RefitChangeType Type, bool IsDone, ChassisLocations Location)
    : RefitChange(Type, IsDone);

/// <summary>A step that adds or removes a location's armor.</summary>
/// <param name="Armor">The location's armor after the change, by side.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ModifyArmorChange(bool IsDone, ChassisLocations Location, LocationArmor<float> Armor)
    : LocationRefitChange(RefitChangeType.ModifyArmor, IsDone, Location);

/// <summary>A step that repairs a location's structure.</summary>
/// <param name="Structure">The structure points restored.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RepairStructureChange(bool IsDone, ChassisLocations Location, int Structure)
    : LocationRefitChange(RefitChangeType.RepairStructure, IsDone, Location);
