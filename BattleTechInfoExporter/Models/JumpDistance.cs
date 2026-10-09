using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>How far a mech jumps with a number of working jump jets.</summary>
/// <param name="JumpJets">The number of working jump jets, from 1; more than the last entry jump as far as it.</param>
/// <param name="Distance">
///     The distance in meters, before the jump jets' <c>JumpDistanceMultiplier</c> effects, which add to a
///     multiplier of 1.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record JumpDistance(int JumpJets, float Distance);
