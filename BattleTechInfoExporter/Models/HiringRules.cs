using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     Hiring and paying pilots. A pilot's skill total is the sum of their four skills; their base skills are the
///     ones they were hired with, the bonus skills the ones trained since.
/// </summary>
/// <param name="CostPerBaseSkillPoint">The hiring price, before the system owner's reputation adjustment.</param>
/// <param name="SalaryPerBaseSkillPoint">Per financial report.</param>
/// <param name="SalaryPerBonusSkillPoint">Per financial report.</param>
/// <param name="PilotsPerSystem">How many pilots a system's hiring hall offers.</param>
/// <param name="RoninChance">The chance of each of them being a ronin, unless the system sets its own.</param>
/// <param name="GeneratedPilotHealth">The health, the injuries they can take, of the pilots the game generates.</param>
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
    IReadOnlyList<MoraleHiringLimit> MoraleLimits,
    int MaxPilotsPerBarracksPod,
    int MaxMechsPerMechBayPod);

/// <summary>Below a company morale, only pilots up to a skill total can be hired; the first matching limit applies.</summary>
/// <param name="BelowMorale">The company morale below which the limit applies.</param>
/// <param name="MaxHireableSkillTotal">The highest skill total of a pilot the company can hire under the limit.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MoraleHiringLimit(int BelowMorale, int MaxHireableSkillTotal);
