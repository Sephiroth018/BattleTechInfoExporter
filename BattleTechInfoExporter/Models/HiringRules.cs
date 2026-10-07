using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     Hiring and paying pilots. A pilot's skill total is the sum of their four skills; their base skills are the
///     ones they were hired with, the bonus skills the ones trained since.
/// </summary>
/// <param name="CostPerBaseSkillPoint">The hiring cost, before the system owner's reputation adjustment.</param>
/// <param name="SalaryPerBaseSkillPoint">Per financial report.</param>
/// <param name="SalaryPerBonusSkillPoint">Per financial report.</param>
/// <param name="PilotsPerSystem">How many pilots a system's hiring hall offers.</param>
/// <param name="RoninChance">The chance of each of them being a ronin, unless the system sets its own.</param>
/// <param name="GeneratedPilotHealth">The health, the injuries they can take, of the pilots the game generates.</param>
/// <param name="MercenaryReviewBoardLevels">
///     The rating levels, which <see cref="MercenaryReviewBoard.Level" /> refers to.
/// </param>
/// <param name="MoraleLimits">Low company morale limits who can be hired.</param>
/// <param name="MaxPilotsPerBarracksPod">The roster size each barracks pod of the ship allows.</param>
/// <param name="MaxMechsPerMechBayPod">The mech bays each mech bay pod of the ship allows.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HiringRules(
    float CostPerBaseSkillPoint,
    float SalaryPerBaseSkillPoint,
    float SalaryPerBonusSkillPoint,
    int PilotsPerSystem,
    float RoninChance,
    int GeneratedPilotHealth,
    IReadOnlyList<MercenaryReviewBoardLevel> MercenaryReviewBoardLevels,
    IReadOnlyList<MoraleHiringLimit> MoraleLimits,
    int MaxPilotsPerBarracksPod,
    int MaxMechsPerMechBayPod);

/// <param name="StartsAt">The lowest rating value in the level.</param>
/// <param name="MaxHireableSkillTotal">The highest skill total of a pilot the company can hire at this level.</param>
/// <param name="ContractReputationMultiplier">
///     Multiplies a contract's difficulty into the rating gained from it.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MercenaryReviewBoardLevel(
    int Level,
    int StartsAt,
    int MaxHireableSkillTotal,
    float ContractReputationMultiplier);

/// <summary>Below a company morale, only pilots up to a skill total can be hired; the first matching limit applies.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MoraleHiringLimit(int BelowMorale, int MaxHireableSkillTotal);
