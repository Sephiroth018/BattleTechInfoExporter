using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="StarSystem">
///     The star system the ship is in, in full, as in <c>star-systems.json</c>; while travelling, the one it left or,
///     once it has jumped, the last it jumped to.
/// </param>
/// <param name="TravelStatus">Where the ship is on its way, e.g. <c>IN_SYSTEM</c> or <c>TRANSIT_TO_JUMP</c>.</param>
/// <param name="Travel">Where the ship is headed; <c>null</c> while it's in a star system.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Position(StarSystem StarSystem, SimGameTravelStatus TravelStatus, Travel? Travel);
