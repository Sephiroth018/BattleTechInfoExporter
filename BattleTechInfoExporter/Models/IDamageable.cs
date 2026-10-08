using BattleTech;

namespace BattleTechInfoExporter.Models;

/// <summary>A component in a state that carries its damage.</summary>
internal interface IDamageable
{
    ComponentDamageLevel DamageLevel { get; }
}
