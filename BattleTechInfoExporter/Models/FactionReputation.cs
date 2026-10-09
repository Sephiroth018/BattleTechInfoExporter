using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Faction">The faction the reputation is with.</param>
/// <param name="Value">The reputation points with the faction, which <paramref name="Level" /> follows from.</param>
/// <param name="Level">The reputation level, described in <see cref="Rules.ReputationLevels" />.</param>
/// <param name="IsAllied">Whether the company is allied with the faction.</param>
/// <param name="IsEnemy">Whether the faction is an enemy of a faction the company is allied with.</param>
/// <param name="IsDisplayed">Whether the game's reputation screen lists the faction; story events add and remove factions.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record FactionReputation(
    DefinitionReference Faction,
    int Value,
    SimGameReputation Level,
    bool IsAllied,
    bool IsEnemy,
    bool IsDisplayed);
