using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The combat effect of a pilot's spirits: the resolve their mech's Precision Strike and Vigilance cost.</summary>
/// <param name="PrecisionStrikeCost">The resolve a Precision Strike costs.</param>
/// <param name="VigilanceCost">The resolve Vigilance costs.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SpiritsLevelCosts(SpiritsLevel Level, int PrecisionStrikeCost, int VigilanceCost);
