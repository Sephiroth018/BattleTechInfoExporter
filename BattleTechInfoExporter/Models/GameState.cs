using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The root of <c>game-state.json</c>.</summary>
/// <param name="ActiveContract">The travel contract the company has accepted; <c>null</c> without one.</param>
/// <param name="Contracts">
///     The contracts the Command Center offers, without the active contract; <c>null</c> until the game has
///     generated the current system's contracts, which it does when the contract screen first opens in a system or
///     after a contract.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record GameState(
    string ModVersion,
    ExportTrigger Trigger,
    Company Company,
    IReadOnlyList<BarracksPilot> Pilots,
    IReadOnlyList<Mech> Mechs,
    IReadOnlyList<LastLanceUnit> LastLance,
    Storage Storage,
    Stores Stores,
    IReadOnlyList<HiringHallPilot> HiringHall,
    ActiveContract? ActiveContract,
    IReadOnlyList<Contract>? Contracts,
    ComponentDefinitions ComponentDefinitions,
    Position Position) : ExportFile(ModVersion, null, Trigger);
