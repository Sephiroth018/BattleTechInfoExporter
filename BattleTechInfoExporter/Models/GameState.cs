using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The root of <c>game-state.json</c>.</summary>
/// <param name="Company">The company's day, funds, morale, reputation and techs.</param>
/// <param name="FinancialReport">The next financial report, as the finance screen projects it.</param>
/// <param name="Ship">The Argo; <c>null</c> when the company still flies the Leopard.</param>
/// <param name="WorkQueue">
///     The timeline's entries, by the day they finish; entries finishing on the same day are in the order mech lab,
///     med bay, travel, financial report, ship upgrade.
/// </param>
/// <param name="Pilots">The pilots in the barracks, the commander first.</param>
/// <param name="Mechs">The mechs in the mech bay, readying ones included, by bay row and position.</param>
/// <param name="MechsAwaitingPlacement">The new mechs the game asks to place, store or scrap, in order.</param>
/// <param name="LastLance">The units of the lance last sent on a mission.</param>
/// <param name="Storage">What the company keeps in storage.</param>
/// <param name="Stores">
///     The current star system's stores, each <c>null</c> when the star system has none or the company can't use
///     it. Their stock changes only on arrival in a star system.
/// </param>
/// <param name="HiringHall">The pilots for hire in the current star system's hiring hall.</param>
/// <param name="ActiveContract">The travel contract the company has accepted; <c>null</c> when there's none.</param>
/// <param name="Contracts">
///     The contracts the Command Center offers, without the active contract, generated for the export when the game
///     hasn't yet; during a trip, only from the jump into the destination on.
/// </param>
/// <param name="Position">Where the ship is: the star system, and the travel under way.</param>
/// <param name="Events">
///     The events scheduled, drawn lately or used up, and the company's tags and statistics their requirements check.
/// </param>
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
    StarSystemStores Stores,
    IReadOnlyList<HiringHallPilot> HiringHall,
    ActiveContract? ActiveContract,
    IReadOnlyList<OfferedContract> Contracts,
    Position Position,
    EventState Events) : ExportFile
{
    internal const string FileName = "game-state.json";
}
