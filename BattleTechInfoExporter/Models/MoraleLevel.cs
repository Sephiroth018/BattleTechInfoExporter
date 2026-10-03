using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="ResolvePerTurn">
///     The morale level's share of the resolve gained per combat turn; the lance's own bonuses
///     add to it.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MoraleLevel(string Name, int StartsAt, int ResolvePerTurn);
