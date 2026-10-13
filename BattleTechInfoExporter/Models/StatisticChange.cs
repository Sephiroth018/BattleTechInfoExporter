using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A change an ability or a component makes to a statistic of the unit, its pilot or its components.</summary>
/// <param name="Statistic">The statistic's name, as the game names it.</param>
/// <param name="Operation">How <see cref="Value" /> changes the statistic, e.g. <c>Set</c>, <c>Int_Add</c>.</param>
/// <param name="Value">
///     A boolean, integer or number, as the statistic's type requires. Changes to the initiative statistics
///     (<c>BaseInitiative</c>, <c>PhaseModifier</c>, <c>PhaseModifierSelf</c>) are as the HUD numbers phases, so a
///     positive change makes the unit act earlier.
/// </param>
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

/// <param name="Collection">
///     The pilot, the weapons, the ammunition boxes, one random working weapon or the strongest working weapon.
/// </param>
/// <param name="Filter">
///     Which of the weapons or ammunition boxes; <c>null</c> for all of them, and always for the pilot, the random
///     and the strongest weapon.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EffectTargetCollection(StatisticEffectData.TargetCollection Collection, ComponentFilter? Filter);

/// <summary>
///     The one property the game filters the components by; the game's data can name several, of which only the
///     first in <see cref="ComponentFilterKind" />'s order counts. Each kind has a record with its value's type.
/// </summary>
/// <param name="Kind">Which property of the components the filter compares, deciding the type of its value.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record ComponentFilter(ComponentFilterKind Kind);

/// <param name="Value">The weapon sub type the weapons must have, e.g. <c>Melee</c> or <c>DFA</c>.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record WeaponSubTypeFilter(WeaponSubType Value) : ComponentFilter(ComponentFilterKind.WeaponSubType);

/// <param name="Value">The weapon type the weapons must have, e.g. <c>LRM</c> or <c>SRM</c>.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record WeaponTypeFilter(WeaponType Value) : ComponentFilter(ComponentFilterKind.WeaponType);

/// <param name="Value">The weapon category the weapons must have, as the catalog's weapons name theirs.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record WeaponCategoryFilter(string Value) : ComponentFilter(ComponentFilterKind.WeaponCategory);

/// <param name="Value">
///     The ammunition category the ammunition boxes must have, as the catalog's ammunition boxes name theirs.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record AmmoCategoryFilter(string Value) : ComponentFilter(ComponentFilterKind.AmmoCategory);

/// <param name="Count">How many of <see cref="Unit" /> the change lasts, at least 1.</param>
/// <param name="Unit">What the duration counts.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EffectDuration(int Count, EffectDurationUnit Unit);
