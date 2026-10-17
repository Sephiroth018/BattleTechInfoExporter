using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     How a COIL weapon gains from its mech's evasion pips; the weapon's own values are its
///     <see cref="WeaponDefinition.Coil" />.
/// </summary>
/// <param name="MultipliesShots">
///     Whether the pips multiply the shots fired, never below the base; when not, once the mech has moved this round
///     and not sprinted, the damage of one shot is multiplied by the pips to the power of
///     <see cref="CoilWeapon.EvasiveExponent" />, never below the base damage.
/// </param>
/// <param name="CountsJumps">Whether the pips count after a jump; when not, a jump leaves the weapon at its base.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CoilRules(bool MultipliesShots, bool CountsJumps);
