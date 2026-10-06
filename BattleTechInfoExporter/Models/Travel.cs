using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="ArrivesOnDay">The day the ship arrives, on the scale of <see cref="Company.DaysPassed" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Travel(DefinitionReference Destination, int ArrivesOnDay);
