using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The company's morale and the level it falls in.</summary>
/// <param name="Value">
///     The company's morale, which each level's <see cref="MoraleLevel.StartsAt" /> is compared with.
/// </param>
/// <param name="Level">The name of the current level in <see cref="Rules.MoraleLevels" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Morale(int Value, string Level);
