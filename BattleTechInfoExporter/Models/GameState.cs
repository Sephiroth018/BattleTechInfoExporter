using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The root of <c>game-state.json</c>.</summary>
/// <param name="Contracts">
///     The contracts the Command Center offers; <c>null</c> until the game has generated the current system's
///     contracts, which it does when the contract screen first opens in a system or after a contract.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record GameState(
    string ModVersion,
    DateTimeOffset ExportedAt,
    ExportTrigger Trigger,
    Company Company,
    IReadOnlyList<BarracksPilot> Pilots,
    IReadOnlyList<Mech> Mechs,
    IReadOnlyList<LastLanceUnit> LastLance,
    Storage Storage,
    Stores Stores,
    IReadOnlyList<HiringHallPilot> HiringHall,
    IReadOnlyList<Contract>? Contracts,
    ComponentDefinitions ComponentDefinitions,
    Position Position,
    Rules Rules);
