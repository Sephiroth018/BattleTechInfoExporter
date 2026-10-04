using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>One step of a mech's queued refit; only the values its <see cref="Type" /> uses are set.</summary>
/// <param name="IsDone">Whether the mech techs have finished the step; its change is then part of the mech.</param>
/// <param name="Component">The component installed, removed or repaired.</param>
/// <param name="DamageLevel">The component's damage level when the step was ordered.</param>
/// <param name="Location">
///     Where the component goes or comes from, where the repaired component is mounted (<c>null</c> if it isn't),
///     or the location whose armor or structure changes.
/// </param>
/// <param name="FrontArmor">The location's front armor after the change.</param>
/// <param name="RearArmor">The location's rear armor after the change; <c>null</c> outside the torso.</param>
/// <param name="Structure">The structure points restored.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RefitChange(
    RefitChangeType Type,
    bool IsDone,
    DefinitionReference? Component = null,
    ComponentDamageLevel? DamageLevel = null,
    ChassisLocations? Location = null,
    int? FrontArmor = null,
    int? RearArmor = null,
    int? Structure = null);
