using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The company's morale and the level it falls in.</summary>
/// <param name="Value">
///     The company's morale; <paramref name="Level" /> is the highest whose <see cref="MoraleLevel.StartsAt" /> it
///     reaches.
/// </param>
/// <param name="Level">The name of the current level in <see cref="Rules.MoraleLevels" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Morale(int Value, string Level);
