using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Value">The company's morale, which <see cref="MoraleLevel.StartsAt" /> compares with.</param>
/// <param name="Level">The name of the current level in <see cref="Rules.MoraleLevels" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Morale(int Value, string Level);
