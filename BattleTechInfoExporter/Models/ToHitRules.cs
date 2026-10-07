using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     How an attack's hit chance follows from its difficulty: the base chance of the skill level (see
///     <see cref="SkillLevel.BaseHitChancePercent" />) loses <see cref="DifficultySteps" /> per point of the
///     summed modifiers, is rounded to <see cref="RoundedToPercent" /> and clamped between
///     <see cref="MinChancePercent" /> and <see cref="MaxChancePercent" />.
/// </summary>
/// <param name="CanTotalModifierBeNegative">
///     Whether bonuses can push the chance above the base chance; when not, they only cancel penalties.
/// </param>
/// <param name="RangeBands">The modifier of each range band of the weapon.</param>
/// <param name="TargetWeightClass">The modifier for the target's size.</param>
/// <param name="TargetShutdown">Against a shut-down mech, ranged attacks.</param>
/// <param name="TargetProne">Against a prone mech, ranged attacks.</param>
/// <param name="AttackerSprinted">When the attacker sprinted this round.</param>
/// <param name="AttackerWalked">When the attacker moved more than 10 meters this round without sprinting.</param>
/// <param name="AttackerStoodUp">When the attacker stood up this round.</param>
/// <param name="AttackerOverheated">When the attacking mech is overheated.</param>
/// <param name="ArmMountedWeapon">For a mech's ranged weapon mounted in an arm.</param>
/// <param name="WeaponInDamagedLocation">For a weapon in a damaged location of a mech.</param>
/// <param name="MechFiringArcDegrees">
///     How far to either side of its facing a mech can fire without turning; a turret's arc is in the catalog.
/// </param>
/// <param name="TurretGunnery">The gunnery every turret shoots with; turrets have no pilot.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ToHitRules(
    IReadOnlyList<DifficultyStep> DifficultySteps,
    int RoundedToPercent,
    int MinChancePercent,
    int MaxChancePercent,
    bool CanTotalModifierBeNegative,
    RangeBandModifiers RangeBands,
    TargetWeightClassModifiers TargetWeightClass,
    ElevationModifier Elevation,
    float TargetShutdown,
    float TargetProne,
    float AttackerSprinted,
    AttackerWalkedModifiers AttackerWalked,
    float AttackerStoodUp,
    float AttackerOverheated,
    float ArmMountedWeapon,
    float DamagedWeapon,
    DamagedLocationModifiers WeaponInDamagedLocation,
    float PrecisionStrike,
    float MechFiringArcDegrees,
    int TurretGunnery);

/// <summary>The hit chance lost per point of difficulty from <see cref="FromDifficulty" /> up to the next step.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record DifficultyStep(int FromDifficulty, float ChanceLostPerPoint);

/// <summary>The modifier by the target's distance against the weapon's ranges in the catalog.</summary>
/// <param name="WithinMinimumRange">Closer than the weapon's minimum range.</param>
/// <param name="Maximum">Beyond the long range, up to the maximum range.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RangeBandModifiers(
    float WithinMinimumRange,
    float Short,
    float Medium,
    float Long,
    float Maximum);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record TargetWeightClassModifiers(
    ByWeightClass<float> Mech,
    ByWeightClass<float> Vehicle,
    ByWeightClass<float> Turret);

/// <summary>
///     The modifier for the height difference: <see cref="ModifierPerLevel" /> per full <see cref="LevelHeight" />
///     the attacker stands above the target.
/// </summary>
/// <param name="LevelHeight">In meters.</param>
/// <param name="UsesMultipleLevels">Whether more than one level counts.</param>
/// <param name="AppliesPenalties">Whether an attacker below the target is penalized.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ElevationModifier(
    float LevelHeight,
    float ModifierPerLevel,
    bool UsesMultipleLevels,
    bool AppliesPenalties);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record AttackerWalkedModifiers(ByWeightClass<float> Mech, float Vehicle);

/// <param name="Penalized">The location is damaged.</param>
/// <param name="NonFunctional">The location is damaged beyond use.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record DamagedLocationModifiers(float Penalized, float NonFunctional);
