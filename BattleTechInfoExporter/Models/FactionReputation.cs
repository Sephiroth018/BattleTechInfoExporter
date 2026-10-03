using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record FactionReputation(
    DefinitionReference Faction,
    int Value,
    SimGameReputation Level,
    bool IsAllied,
    bool IsEnemy);
