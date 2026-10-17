using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>How far a mech jumps with a number of working jump jets.</summary>
/// <param name="JumpJets">The number of working jump jets, from 1; more than the last entry jump as far as it.</param>
/// <param name="Distance">
///     The distance in meters, before it is multiplied by 1 plus the jump jets' <c>JumpDistanceMultiplier</c>
///     effects.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record JumpDistance(int JumpJets, float Distance);
