using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A change an event made to a value for a number of days; the value includes it until it ends.</summary>
/// <param name="EndsOnDay">
///     The last day the change applies, on the scale of <see cref="Company.DaysPassed" />; the game reverts it as the
///     next day starts.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record TemporaryChange(int Amount, int EndsOnDay);
