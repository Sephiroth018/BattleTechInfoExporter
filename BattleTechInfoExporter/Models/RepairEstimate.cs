using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>What the mech bay's Repair would take if ordered now, as its confirmation shows it.</summary>
/// <param name="Days">
///     The days the mech lab needs for the repair alone; it works on one order at a time, so an order queued behind
///     others finishes later.
/// </param>
/// <param name="Cost">The C-Bills the repair costs.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RepairEstimate(int Days, int Cost);
