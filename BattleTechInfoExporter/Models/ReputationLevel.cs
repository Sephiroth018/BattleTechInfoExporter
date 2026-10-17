using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A faction reputation level and its effects.</summary>
/// <param name="Level">The level as the game names it, e.g. <c>LIKED</c>.</param>
/// <param name="StartsAt">The lowest reputation value in the level.</param>
/// <param name="MaxContractDifficulty">
///     The highest difficulty of the faction's contracts offered at this level, including the career's current
///     global difficulty. Story, restoration and flashpoint contracts aren't limited.
/// </param>
/// <param name="CanUseSystemStore">Whether the system store is open at this level with the star system's owner.</param>
/// <param name="StorePriceChangePercent">
///     The change of store prices at this level with the store's faction (the star system's owner, or the pirates
///     at the black market), as a percentage of an item's cost added to its price, as the UI shows it. It adds to
///     the star system's own discount.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ReputationLevel(
    SimGameReputation Level,
    int StartsAt,
    int MaxContractDifficulty,
    bool CanUseSystemStore,
    int StorePriceChangePercent);
