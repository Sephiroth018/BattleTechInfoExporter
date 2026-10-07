using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A change an ability or a component makes to a statistic of the unit, its pilot or its components.</summary>
/// <param name="Value">A boolean, integer or number, as the statistic's type requires.</param>
/// <param name="AppliesTo">
///     The pilot or the components whose statistic changes; <c>null</c> for the unit's own.
/// </param>
/// <param name="Duration">How long the change lasts; <c>null</c> when it lasts for the whole mission.</param>
/// <param name="Target">
///     Whose statistic changes: the unit with the ability or component (<c>Creator</c>) or the unit it targets.
/// </param>
/// <param name="Trigger">When the change is made, e.g. on the ability's activation, on a hit or always (<c>Passive</c>).</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StatisticChange(
    string Statistic,
    StatCollection.StatOperation Operation,
    object Value,
    EffectTargetCollection? AppliesTo,
    EffectDuration? Duration,
    EffectTargetType Target,
    EffectTriggerType Trigger);

/// <param name="WeaponCategory">The weapon category, as the catalog's weapons name theirs.</param>
/// <param name="AmmoCategory">The ammunition category, as the catalog's ammunition boxes name theirs.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EffectTargetCollection(
    StatisticEffectData.TargetCollection Collection,
    WeaponSubType? WeaponSubType,
    WeaponType? WeaponType,
    string? WeaponCategory,
    string? AmmoCategory);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EffectDuration(int Count, EffectDurationUnit Unit);
