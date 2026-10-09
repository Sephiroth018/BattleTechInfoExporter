using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A component mounted on a mech in the mech bay.</summary>
/// <param name="DamageLevel">The component's damage, e.g. <c>Functional</c> or <c>Destroyed</c>.</param>
/// <param name="IsFixed">
///     Whether it's part of the chassis: every mech of the chassis has it, and it can't be removed.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MountedComponent(ComponentReference Component, ComponentDamageLevel DamageLevel, bool IsFixed)
    : ComponentEntry(Component), IDamageable, IFixedOrRemovable;
