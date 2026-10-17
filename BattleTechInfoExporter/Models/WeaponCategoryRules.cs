using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>What a weapon category's attacks do against units of each kind.</summary>
/// <param name="VehicleDamageMultiplier">
///     Multiplies the damage and structure damage the category's hits deal to a vehicle.
/// </param>
/// <param name="TurretDamageMultiplier">
///     Multiplies the damage and structure damage the category's hits deal to a turret.
/// </param>
/// <param name="IsAffectedByEvasion">
///     Whether the target's evasion pips apply to the category's attacks; the weapon's type must also be one of
///     <see cref="EvasionRules.WeaponTypesAffected" />.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record WeaponCategoryRules(
    float VehicleDamageMultiplier,
    float TurretDamageMultiplier,
    bool IsAffectedByEvasion);
