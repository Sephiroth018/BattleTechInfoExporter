using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The root of <c>game-state.json</c>.</summary>
/// <param name="Company">The company's day, funds, morale, reputation and techs.</param>
/// <param name="FinancialReport">The next financial report, as the finance screen projects it.</param>
/// <param name="Ship">The Argo; <c>null</c> while the company still flies the Leopard.</param>
/// <param name="WorkQueue">
///     The timeline's entries, by the day they finish; entries finishing on the same day are in the order mech lab,
///     med bay, travel, financial report, ship upgrade.
/// </param>
/// <param name="Pilots">The pilots in the barracks, the commander first.</param>
/// <param name="Mechs">The mechs in the mech bay, readying ones included, by bay row and position.</param>
/// <param name="MechsAwaitingPlacement">The new mechs the game asks to place, store or scrap, in order.</param>
/// <param name="LastLance">The units of the lance last sent on a mission.</param>
/// <param name="Storage">What the company keeps in storage.</param>
/// <param name="Stores">The current system's stores.</param>
/// <param name="HiringHall">The pilots for hire in the current system's hiring hall.</param>
/// <param name="ActiveContract">The travel contract the company has accepted; <c>null</c> without one.</param>
/// <param name="Contracts">
///     The contracts the Command Center offers, without the active contract; <c>null</c> until the game has
///     generated the current system's contracts, which it does when the contract screen first opens in a system or
///     after a contract.
/// </param>
/// <param name="Position">Where the ship is: the star system, and the travel under way.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record GameState(
    Company Company,
    FinancialReport FinancialReport,
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
    IReadOnlyList<OfferedContract>? Contracts,
    Position Position) : ExportFile
{
    internal const string FileName = "game-state.json";
}
