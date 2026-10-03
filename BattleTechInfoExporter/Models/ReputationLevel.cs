using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A faction reputation level and its effects.</summary>
/// <param name="StartsAt">The lowest reputation value in the level.</param>
/// <param name="MaxContractDifficulty">
///     The highest difficulty of the faction's contracts offered at this level, including the career's current
///     global difficulty. Story, restoration and flashpoint contracts aren't limited.
/// </param>
/// <param name="StorePriceAdjustment">
///     The fraction of an item's cost added to its price in a store of the system owner at this level.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ReputationLevel(
    SimGameReputation Level,
    int StartsAt,
    int MaxContractDifficulty,
    float StorePriceAdjustment);
