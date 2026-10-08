using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>One step of a mech's queued refit; each <see cref="Type" /> has a record with the fields of its own.</summary>
/// <param name="IsDone">Whether the mech techs have finished the step; its change is then part of the mech.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record RefitChange(RefitChangeType Type, bool IsDone);

/// <summary>A step that installs, removes or repairs a component.</summary>
/// <param name="DamageLevel">The component's damage level when the step was ordered.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record ComponentRefitChange(
    RefitChangeType Type,
    bool IsDone,
    ComponentReference Component,
    ComponentDamageLevel DamageLevel) : RefitChange(Type, IsDone);

/// <param name="Location">Where the component goes.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record InstallComponentChange(
    bool IsDone,
    ComponentReference Component,
    ComponentDamageLevel DamageLevel,
    ChassisLocations Location)
    : ComponentRefitChange(RefitChangeType.InstallComponent, IsDone, Component, DamageLevel);

/// <param name="Location">Where the component comes from.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RemoveComponentChange(
    bool IsDone,
    ComponentReference Component,
    ComponentDamageLevel DamageLevel,
    ChassisLocations Location)
    : ComponentRefitChange(RefitChangeType.RemoveComponent, IsDone, Component, DamageLevel);

/// <param name="Location">Where the component is mounted; <c>null</c> for one in storage.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RepairComponentChange(
    bool IsDone,
    ComponentReference Component,
    ComponentDamageLevel DamageLevel,
    ChassisLocations? Location)
    : ComponentRefitChange(RefitChangeType.RepairComponent, IsDone, Component, DamageLevel);

/// <summary>A step that changes a location's armor or structure.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record LocationRefitChange(RefitChangeType Type, bool IsDone, ChassisLocations Location)
    : RefitChange(Type, IsDone);

/// <param name="FrontArmor">The location's front armor after the change.</param>
/// <param name="RearArmor">The location's rear armor after the change; <c>null</c> outside the torso.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ModifyArmorChange(bool IsDone, ChassisLocations Location, int FrontArmor, int? RearArmor)
    : LocationRefitChange(RefitChangeType.ModifyArmor, IsDone, Location);

/// <param name="Structure">The structure points restored.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RepairStructureChange(bool IsDone, ChassisLocations Location, int Structure)
    : LocationRefitChange(RefitChangeType.RepairStructure, IsDone, Location);
