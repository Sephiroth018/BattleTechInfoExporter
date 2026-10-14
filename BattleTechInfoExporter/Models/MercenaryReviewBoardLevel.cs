using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Level">The rating level, from 0, which <see cref="MercenaryReviewBoard.Level" /> refers to.</param>
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
