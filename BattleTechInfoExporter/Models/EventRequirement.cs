using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     What one scope's tags and statistics must be for an event to come, an option to be open or a result to apply.
/// </summary>
/// <param name="Scope">
///     Whose tags and statistics are checked: the company's or a pilot's, the commander included (both in
///     <c>game-state.json</c>), or the current star system's (in <c>star-systems.json</c>).
/// </param>
/// <param name="RequiredTags">The tags that must all be present.</param>
/// <param name="ExcludedTags">The tags none of which may be present.</param>
/// <param name="Comparisons">The statistic values that must all hold.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EventRequirement(
    EventScope Scope,
    IReadOnlyList<string> RequiredTags,
    IReadOnlyList<string> ExcludedTags,
    IReadOnlyList<StatisticComparison> Comparisons);

/// <param name="Statistic">The statistic's name, as the game names it; a statistic the scope doesn't have counts as 0.</param>
/// <param name="Operator">How the statistic's value compares to <paramref name="Value" />.</param>
/// <param name="Value">The value the statistic is compared to.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StatisticComparison(string Statistic, Operator Operator, float Value);
