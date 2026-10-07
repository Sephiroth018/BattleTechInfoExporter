using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>How far a mech jumps with a number of working jump jets.</summary>
/// <param name="Distance">
///     The distance in meters, before the jump jets' <c>JumpDistanceMultiplier</c> effects, which add to a
///     multiplier of 1.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record JumpDistance(int JumpJets, float Distance);
