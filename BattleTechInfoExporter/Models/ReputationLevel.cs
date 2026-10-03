using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A faction reputation level and its effects.</summary>
/// <param name="StartsAt">The lowest reputation value in the level.</param>
/// <param name="MaxContractDifficulty">
///     The highest difficulty of the faction's contracts offered at this level, including the career's current
///     global difficulty. Story, restoration and flashpoint contracts aren't limited.
/// </param>
/// <param name="CanUseStore">Whether the system owner's store is open at this level.</param>
/// <param name="StorePriceAdjustment">
///     The fraction of an item's cost added to its price in the system owner's store at this level, or
///     <see langword="null" /> where the store is closed.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ReputationLevel(
    SimGameReputation Level,
    int StartsAt,
    int MaxContractDifficulty,
    bool CanUseStore,
    float? StorePriceAdjustment);
