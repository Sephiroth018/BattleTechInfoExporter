using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The resolve a mech's Precision Strike and Vigilance cost.</summary>
/// <param name="PrecisionStrikeCost">The resolve a Precision Strike costs.</param>
/// <param name="VigilanceCost">The resolve Vigilance costs.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal record ResolveCosts(int PrecisionStrikeCost, int VigilanceCost);
