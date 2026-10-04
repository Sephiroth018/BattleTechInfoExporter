using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>How the game works for this career: level tables that values in the other sections refer to.</summary>
/// <param name="MoraleLevels">The morale levels, which <see cref="Morale.Level" /> refers to.</param>
/// <param name="ReputationLevels">The reputation levels, which <see cref="FactionReputation.Level" /> refers to.</param>
/// <param name="Skills">The training table behind <see cref="Pilot.Skills" /> and <see cref="Pilot.Abilities" />.</param>
/// <param name="SpiritsLevels">The effects of each spirits level, which <see cref="Spirits.Level" /> refers to.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Rules(
    IReadOnlyList<MoraleLevel> MoraleLevels,
    IReadOnlyList<ReputationLevel> ReputationLevels,
    SkillRules Skills,
    IReadOnlyList<SpiritsLevelCosts> SpiritsLevels);
