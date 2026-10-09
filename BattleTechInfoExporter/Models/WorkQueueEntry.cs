using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>An entry of the timeline: what the company is waiting for.</summary>
/// <param name="Type">The game's kind of work order, e.g. <c>MedLabHeal</c> or <c>FinancialReport</c>.</param>
/// <param name="FinishesOnDay">The day it finishes, on the scale of <see cref="Company.DaysPassed" />.</param>
/// <param name="Target">
///     What the entry is about, by its <paramref name="Type" />: the mech the mech lab works on, the pilot the med bay
///     heals, the star system the ship travels to or the ship upgrade being installed, as in
///     <see cref="Ship.Upgrades" />. <c>null</c> for the financial report, and where the game can't resolve it.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record WorkQueueEntry(WorkOrderType Type, int FinishesOnDay, Reference? Target);
