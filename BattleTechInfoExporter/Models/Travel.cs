using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Destination">
///     The star system the ship travels to, at the end of its route, in full, as in <c>star-systems.json</c>.
/// </param>
/// <param name="ArrivesOnDay">The day the ship arrives, on the scale of <see cref="Company.DaysPassed" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Travel(StarSystem Destination, int ArrivesOnDay);
