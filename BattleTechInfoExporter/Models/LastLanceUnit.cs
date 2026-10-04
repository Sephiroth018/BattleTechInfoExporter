using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A mech and its pilot from the lance last sent on a mission. The lance configuration pre-fills from it the
///     mechs that can be fielded and the pilots who can pilot, in the slots the contract leaves open.
/// </summary>
/// <param name="Mech">The mech; <c>null</c> when it has left the mech bay since, e.g. sold.</param>
/// <param name="Pilot">The pilot; <c>null</c> when they have left the company since, e.g. dismissed or killed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LastLanceUnit(MechReference? Mech, PilotReference? Pilot);
