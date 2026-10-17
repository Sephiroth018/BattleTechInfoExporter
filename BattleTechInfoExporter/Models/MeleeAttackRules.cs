using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The melee rules; damage and stability damage are the chassis' in the catalog. A melee attack against a turret,
///     a building, a prone or a shut-down mech always has the maximum hit chance. The damage against vehicles and
///     turrets is multiplied by the <c>Melee</c> weapon category's (see <see cref="CombatRules.WeaponCategories" />).
/// </summary>
/// <param name="DeathFromAboveToHit">The to-hit modifier of a jump attack.</param>
/// <param name="MaxHeightDifference">
///     The largest height difference, in meters, between the attacker's position and the target's for a melee or
///     jump attack.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MeleeAttackRules(float DeathFromAboveToHit, float MaxHeightDifference);
