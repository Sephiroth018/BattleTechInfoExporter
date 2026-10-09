using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     How contracts are generated, paid and rewarded. A contract's pay is <see cref="PayPerDifficulty" /> times
///     the mission type's <see cref="ContractTypeDescription.PayMultiplier" /> times a scale of its difficulty
///     (the sum of 1 / (0.4 n) for n from 1 to the difficulty, less 1.5), varied by up to
///     <see cref="PayVariance" /> either way and rounded to thousands.
/// </summary>
/// <param name="PayPerDifficulty">In C-Bills, before the type's multiplier, the difficulty scale and the variance.</param>
/// <param name="PayVariance">The most the pay varies either way, as a 0-1 fraction of it.</param>
/// <param name="GuaranteedPayShare">The share of the pay the company gets on top of the negotiated share.</param>
/// <param name="GoodFaithPayShare">The share of the pay for a mission not completed, with a good faith effort.</param>
/// <param name="NoFaithPayShare">The share without a good faith effort.</param>
/// <param name="Reputation">The reputation a contract changes.</param>
/// <param name="Experience">The experience a contract's pilots earn.</param>
/// <param name="MaxGlobalDifficulty">The career's global difficulty never exceeds this.</param>
/// <param name="DifficultyVariance">
///     A contract's difficulty is the system's plus the global one, varied by up to this either way.
/// </param>
/// <param name="MaxPerSystem">The most contracts a system offers at once, unless it sets its own.</param>
/// <param name="RenewedPerRefresh">Added to a system's open contract slots every <see cref="RefreshDays" />.</param>
/// <param name="RefreshDays">
///     A system's open contract slots are renewed once more than this many days passed since their last renewal.
/// </param>
/// <param name="RemovedPerCompleted">Taken off a system's open contract slots for each contract completed there.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ContractRules(
    int PayPerDifficulty,
    float PayVariance,
    float GuaranteedPayShare,
    float GoodFaithPayShare,
    float NoFaithPayShare,
    ContractReputationRules Reputation,
    ContractExperienceRules Experience,
    int MaxGlobalDifficulty,
    int DifficultyVariance,
    int MaxPerSystem,
    float RenewedPerRefresh,
    int RefreshDays,
    float RemovedPerCompleted);

/// <summary>
///     The reputation a contract changes: the employer gains the base plus the negotiated share of the negotiable
///     value, the target loses the employer's gain times its multiplier, and the Mercenary Review Board rating
///     gains the difficulty times the rating level's multiplier (see <see cref="MercenaryReviewBoardLevel" />).
/// </summary>
/// <param name="BasePerDifficulty">Rounded, plus <see cref="BaseAddition" />.</param>
/// <param name="BaseAddition">Added to <see cref="BasePerDifficulty" /> times the difficulty, rounded.</param>
/// <param name="NegotiablePerDifficulty">Rounded, plus <see cref="NegotiableAddition" />.</param>
/// <param name="NegotiableAddition">
///     Added to <see cref="NegotiablePerDifficulty" /> times the difficulty, rounded.
/// </param>
/// <param name="Employer">Multiplies the employer's gain.</param>
/// <param name="Target">
///     Multiplies the employer's gain, before <see cref="Employer" />, into the target's change; negative for a loss.
/// </param>
/// <param name="MercenaryReviewBoard">Multiplies the Mercenary Review Board rating gained.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ContractReputationRules(
    float BasePerDifficulty,
    int BaseAddition,
    float NegotiablePerDifficulty,
    int NegotiableAddition,
    OutcomeMultipliers Employer,
    OutcomeMultipliers Target,
    OutcomeMultipliers MercenaryReviewBoard);

/// <summary>What the mission's outcome does to a reward.</summary>
/// <param name="Completed">Completed successfully.</param>
/// <param name="GoodFaith">Not completed, with a good faith effort.</param>
/// <param name="BadFaith">Not completed, without one.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record OutcomeMultipliers(float Completed, float GoodFaith, float BadFaith);

/// <summary>The experience every pilot sent on the mission earns.</summary>
/// <param name="PerDifficulty">Times the contract's difficulty, rounded down.</param>
/// <param name="Failed">Multiplies it for a failed mission.</param>
/// <param name="RetreatedGoodFaith">Multiplies it for a retreat with a good faith effort.</param>
/// <param name="RetreatedBadFaith">Multiplies it for a retreat without one.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ContractExperienceRules(
    float PerDifficulty,
    float Failed,
    float RetreatedGoodFaith,
    float RetreatedBadFaith);
