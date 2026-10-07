using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The melee rules; damage and instability are the chassis' in the catalog. A melee attack against a turret,
///     a building, a prone or a shut-down mech always has the maximum hit chance.
/// </summary>
/// <param name="DeathFromAboveToHit">The to-hit modifier of a jump attack.</param>
/// <param name="MaxHeightDifference">In meters, between the attacker's and the target's position.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MeleeAttackRules(float DeathFromAboveToHit, float MaxHeightDifference);
