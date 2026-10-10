using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Destination">
///     The star system the ship travels to, at the end of its route, in full, as in <c>star-systems.json</c>.
/// </param>
/// <param name="ArrivesOnDay">The day the ship arrives, on the scale of <see cref="Company.DaysPassed" />.</param>
/// <param name="JumpPointDays">
///     The days between the current star system's planet and its jump point, which every <see cref="Route.Days" />
///     from the current star system includes in full.
/// </param>
/// <param name="LegEndsOnDay">
///     The day the leg under way ends, on the scale of <see cref="Company.DaysPassed" />: by
///     <see cref="Position.TravelStatus" />, the jump point is reached, a jump is done or, after the last jump, the
///     planet is reached.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Travel(StarSystem Destination, int ArrivesOnDay, int JumpPointDays, int LegEndsOnDay);
