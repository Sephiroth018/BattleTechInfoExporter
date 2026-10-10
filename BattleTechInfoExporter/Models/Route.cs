using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The trip from the current star system, as the starmap plans and shows it.</summary>
/// <param name="Days">
///     The days the trip takes from the current star system's planet, as the starmap shows them while the ship is
///     there; during a trip, also the part already travelled (see <see cref="Travel.ArrivesOnDay" />). 0 for the
///     current star system.
/// </param>
/// <param name="Cost">The C-Bills the trip costs; 0 for the current star system.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Route(int Days, int Cost);
