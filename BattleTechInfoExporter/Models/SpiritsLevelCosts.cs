using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The combat effect of a pilot's spirits: the resolve their mech's Precision Strike and Vigilance cost.</summary>
/// <param name="Level">The pilot's spirits level the costs apply at.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SpiritsLevelCosts(SpiritsLevel Level, int PrecisionStrikeCost, int VigilanceCost)
    : ResolveCosts(PrecisionStrikeCost, VigilanceCost);
