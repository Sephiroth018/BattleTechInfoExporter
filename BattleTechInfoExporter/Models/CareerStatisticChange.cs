using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A change the career makes to a statistic of the company or a pilot, e.g. by a ship upgrade or an event.</summary>
/// <param name="Statistic">The game's name of the statistic, e.g. "MechTechSkill".</param>
/// <param name="Value">The amount added to the statistic, or the value it is set to when <see cref="IsSet" />.</param>
/// <param name="IsSet">Whether the change sets the statistic to <see cref="Value" /> instead of adding it.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal record CareerStatisticChange(string Statistic, float Value, bool IsSet);
