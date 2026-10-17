using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     An entry of the timeline: a work order the company is waiting for. Each <see cref="Kind" /> has a record of
///     its own, with the target it is about where it has one.
/// </summary>
/// <param name="Kind">What the entry is about, which decides its other fields.</param>
/// <param name="Type">The game's kind of work order, e.g. <c>MedLabHeal</c> or <c>FinancialReport</c>.</param>
/// <param name="FinishesOnDay">The day it finishes, on the scale of <see cref="Company.DaysPassed" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record WorkQueueEntry(WorkQueueEntryKind Kind, WorkOrderType Type, int FinishesOnDay);

/// <summary>A mech lab order, of the <c>MechLab…</c> types.</summary>
/// <param name="Target">The mech the mech lab works on; <c>null</c> where the game can't resolve it.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechLabOrder(WorkOrderType Type, int FinishesOnDay, MechReference? Target)
    : WorkQueueEntry(WorkQueueEntryKind.MechLab, Type, FinishesOnDay);

/// <summary>A med bay order, of type <c>MedLabHeal</c>.</summary>
/// <param name="Target">The pilot the med bay heals; always set, since the order holds the pilot itself.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MedBayOrder(WorkOrderType Type, int FinishesOnDay, PilotReference Target)
    : WorkQueueEntry(WorkQueueEntryKind.MedBay, Type, FinishesOnDay);

/// <summary>A trip, of the <c>Travel…</c> types.</summary>
/// <param name="Target">The star system the ship travels to; <c>null</c> where the game can't resolve it.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record TravelOrder(WorkOrderType Type, int FinishesOnDay, DefinitionReference? Target)
    : WorkQueueEntry(WorkQueueEntryKind.Travel, Type, FinishesOnDay);

/// <summary>A ship upgrade being installed, of the <c>ArgoUpgrade…</c> types.</summary>
/// <param name="Target">
///     The ship upgrade, as in <see cref="Ship.Upgrades" />; <c>null</c> where the game can't resolve it.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ShipUpgradeOrder(WorkOrderType Type, int FinishesOnDay, DefinitionReference? Target)
    : WorkQueueEntry(WorkQueueEntryKind.ShipUpgrade, Type, FinishesOnDay);

/// <summary>The next financial report, of type <c>FinancialReport</c>, which is about nothing in particular.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record FinancialReportOrder(WorkOrderType Type, int FinishesOnDay)
    : WorkQueueEntry(WorkQueueEntryKind.FinancialReport, Type, FinishesOnDay);
