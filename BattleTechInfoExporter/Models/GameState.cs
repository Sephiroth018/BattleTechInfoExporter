using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The root of <c>game-state.json</c>.</summary>
/// <param name="MoraleLevels">The game's static morale levels, which <see cref="Models.Morale.Level" /> refers to.</param>
/// <param name="ReputationLevels">
///     The game's reputation levels, which <see cref="FactionReputation.Level" /> refers to.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record GameState(
    string ModVersion,
    DateTimeOffset ExportedAt,
    ExportTrigger Trigger,
    Company Company,
    Position Position,
    IReadOnlyList<MoraleLevel> MoraleLevels,
    IReadOnlyList<ReputationLevel> ReputationLevels);
