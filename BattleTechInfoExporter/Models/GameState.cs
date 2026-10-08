using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The root of <c>game-state.json</c>.</summary>
/// <param name="Ship">The Argo; <c>null</c> while the company still flies the Leopard.</param>
/// <param name="WorkQueue">
///     The timeline's entries, by the day they finish; entries finishing on the same day are in the order mech lab,
///     med bay, travel, financial report, ship upgrade.
/// </param>
/// <param name="ActiveContract">The travel contract the company has accepted; <c>null</c> without one.</param>
/// <param name="Contracts">
///     The contracts the Command Center offers, without the active contract; <c>null</c> until the game has
///     generated the current system's contracts, which it does when the contract screen first opens in a system or
///     after a contract.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record GameState(
    Company Company,
    IReadOnlyList<WorkQueueEntry> WorkQueue,
    Ship? Ship,
    IReadOnlyList<BarracksPilot> Pilots,
    IReadOnlyList<Mech> Mechs,
    IReadOnlyList<MechAwaitingPlacement> MechsAwaitingPlacement,
    IReadOnlyList<LastLanceUnit> LastLance,
    Storage Storage,
    Stores Stores,
    IReadOnlyList<HiringHallPilot> HiringHall,
    ActiveContract? ActiveContract,
    IReadOnlyList<Contract>? Contracts,
    Position Position) : ExportFile
{
    protected override ExportFile WithoutExportHeader() =>
        (GameState)base.WithoutExportHeader() with { Company = Company.WithoutDay() };
}
