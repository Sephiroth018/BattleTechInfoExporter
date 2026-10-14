using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A level of the company's morale, and what it does in combat.</summary>
/// <param name="Level">The level's name as the UI shows it, which <see cref="Morale.Level" /> refers to.</param>
/// <param name="StartsAt">The lowest company morale in the level.</param>
/// <param name="ResolvePerRound">
///     The morale level's share of the resolve gained per combat round; the lance's own bonuses
///     add to it.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MoraleLevel(string Level, int StartsAt, int ResolvePerRound);
