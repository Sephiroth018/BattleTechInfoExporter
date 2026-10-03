using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="IsDisplayed">Whether the game's reputation screen lists the faction; story events add and remove factions.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record FactionReputation(
    DefinitionReference Faction,
    int Value,
    SimGameReputation Level,
    bool IsAllied,
    bool IsEnemy,
    bool IsDisplayed);
