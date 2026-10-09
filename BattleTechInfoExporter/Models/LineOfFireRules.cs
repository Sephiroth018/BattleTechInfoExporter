using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The line of fire by the share of clear sight lines between attacker and target: clear at
///     <see cref="ClearRatio" /> or more, obstructed at <see cref="ObstructedRatio" /> or more, blocked below.
/// </summary>
/// <param name="ObstructedToHit">The to-hit modifier of an obstructed line of fire.</param>
/// <param name="ObstructedDamageMultiplier">Multiplies the damage through an obstructed line of fire.</param>
/// <param name="IndirectFireToHit">The to-hit modifier of indirect fire, at a blocked line of fire.</param>
/// <param name="IndirectFireDamageMultiplier">Multiplies the damage of indirect fire.</param>
/// <param name="ClearRatio">The share of clear sight lines, from 0 to 1, for a clear line of fire.</param>
/// <param name="ObstructedRatio">The share of clear sight lines, from 0 to 1, for an obstructed line of fire.</param>
/// <param name="MinRatioFromUnits">Units in the way alone can't lower the share below this.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LineOfFireRules(
    float ObstructedToHit,
    float ObstructedDamageMultiplier,
    float IndirectFireToHit,
    float IndirectFireDamageMultiplier,
    float ClearRatio,
    float ObstructedRatio,
    float MinRatioFromUnits);
