using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A component mounted on a mech in the mech bay.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MountedComponent(ComponentReference Component, ComponentDamageLevel DamageLevel, bool IsFixed)
    : ComponentEntry(Component), IDamageable, IFixedOrRemovable;
