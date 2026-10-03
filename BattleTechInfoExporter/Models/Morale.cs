using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Level">The name of the current level in <see cref="GameState.MoraleLevels" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Morale(int Value, string Level);
