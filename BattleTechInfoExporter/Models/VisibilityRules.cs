using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     Spotting and sensors. A unit is seen within the spotter range with a line of sight, and shows as a sensor
///     blip within the sensor range; both are multiplied by the chassis' values, and the sensor range by the target's
///     signature. The blip's detail depends on the spotter's Tactics: 4 shows the unit's type, 7 its details.
/// </summary>
/// <param name="SpotterRange">In meters.</param>
/// <param name="SpotterRangePerTactics">Added to the spotter range per point of Tactics.</param>
/// <param name="SensorRange">In meters.</param>
/// <param name="SensorHysteresis">Added to the sensor range against a target already detected.</param>
/// <param name="ShutdownSpotterMultiplier">A shut-down unit's spotter range, as a share of the base range alone.</param>
/// <param name="ProneSpotterMultiplier">A prone mech's spotter range, as a share of the base range alone.</param>
/// <param name="ShutdownSignatureModifier">Added to a shut-down unit's signature.</param>
/// <param name="ShutdownVisibilityModifier">Added to a shut-down unit's visibility multiplier.</param>
/// <param name="GhostedUnitsHideBlips">Whether a unit in stealth hides from sensors altogether.</param>
/// <param name="SensorLock">What a sensor lock does to its target.</param>
/// <param name="FiredWeaponsEffects">The statistic changes a unit makes to itself when it fires.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record VisibilityRules(
    float SpotterRange,
    float SpotterRangePerTactics,
    float SensorRange,
    float SensorHysteresis,
    float ShutdownSpotterMultiplier,
    float ProneSpotterMultiplier,
    float ShutdownSignatureModifier,
    float ShutdownVisibilityModifier,
    bool GhostedUnitsHideBlips,
    SensorLockRules SensorLock,
    IReadOnlyList<StatisticChange> FiredWeaponsEffects);

/// <summary>What a sensor lock does to its target, besides making it visible to the whole lance.</summary>
/// <param name="EvasivePipsStripped">The evasive pips the target loses; 0 when a sensor lock strips none.</param>
/// <param name="TargetEffects">
///     The statistic changes the target gets, one entry per application of a change the game applies repeatedly.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SensorLockRules(int EvasivePipsStripped, IReadOnlyList<StatisticChange> TargetEffects);
