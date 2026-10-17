using BattleTech;

namespace BattleTechInfoExporter.Models;

/// <summary>A component in a state that carries its damage.</summary>
internal interface IDamageable
{
    /// <summary>The component's damage level, e.g. <c>Penalized</c> or <c>Destroyed</c>.</summary>
    ComponentDamageLevel DamageLevel { get; }
}
