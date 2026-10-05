using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     An entry of the timeline: what the company is waiting for. At most one of the targets is set; the financial
///     report has none. A target the game can't resolve is <c>null</c> too.
/// </summary>
/// <param name="FinishesOnDay">The day it finishes, on the scale of <see cref="Company.DaysPassed" />.</param>
/// <param name="Mech">The mech the mech lab works on.</param>
/// <param name="Pilot">The pilot the med bay heals.</param>
/// <param name="Destination">The system the ship travels to.</param>
/// <param name="ShipUpgrade">The ship upgrade being installed, as in <see cref="Ship.Upgrades" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record WorkQueueEntry(
    WorkOrderType Type,
    int FinishesOnDay,
    MechReference? Mech = null,
    PilotReference? Pilot = null,
    DefinitionReference? Destination = null,
    DefinitionReference? ShipUpgrade = null);
