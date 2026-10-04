using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The root of <c>game-state.json</c>.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record GameState(
    string ModVersion,
    DateTimeOffset ExportedAt,
    ExportTrigger Trigger,
    Company Company,
    IReadOnlyList<BarracksPilot> Pilots,
    IReadOnlyList<Mech> Mechs,
    Storage Storage,
    ComponentDefinitions ComponentDefinitions,
    Position Position,
    Rules Rules);
