using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The root of <c>rules.json</c>: how the game works for this career, level tables that values in
///     <see cref="GameState" /> refer to.
/// </summary>
/// <param name="MoraleLevels">The morale levels, which <see cref="Morale.Level" /> refers to.</param>
/// <param name="ReputationLevels">The reputation levels, which <see cref="FactionReputation.Level" /> refers to.</param>
/// <param name="Skills">The training table behind <see cref="Pilot.Skills" /> and <see cref="Pilot.Abilities" />.</param>
/// <param name="SpiritsLevels">The effects of each spirits level, which <see cref="Spirits.Level" /> refers to.</param>
/// <param name="MechPartsPerMech">The <see cref="StoredMechParts" /> needed to assemble a mech.</param>
/// <param name="ContractTypes">The mission types, which <see cref="ContractIdentity.Type" /> refers to.</param>
/// <param name="JumpDistances">
///     The jump distance by number of working jump jets, from one up; the last entry applies to any higher number.
/// </param>
/// <param name="Combat">The combat rules behind to-hit, heat, stability, hit locations, visibility and resolve.</param>
/// <param name="Campaign">
///     The campaign rules behind the mech lab, med bay, hiring, salvage, finances, contracts and travel.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Rules(
    IReadOnlyList<MoraleLevel> MoraleLevels,
    IReadOnlyList<ReputationLevel> ReputationLevels,
    SkillRules Skills,
    IReadOnlyList<SpiritsLevelCosts> SpiritsLevels,
    int MechPartsPerMech,
    IReadOnlyList<ContractTypeDescription> ContractTypes,
    IReadOnlyList<JumpDistance> JumpDistances,
    CombatRules Combat,
    CampaignRules Campaign) : ExportFile
{
    internal const string FileName = "rules.json";

    protected override bool IsWrittenOnlyWhenChanged => true;
}
