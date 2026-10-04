using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The root of <c>game-state.json</c>.</summary>
/// <param name="ComponentDefinitions">
///     The definitions of the components the other sections refer to, keyed by component id. A reference whose
///     definition the game can't find has no entry; its id stands in for its name.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record GameState(
    string ModVersion,
    DateTimeOffset ExportedAt,
    ExportTrigger Trigger,
    Company Company,
    IReadOnlyList<Pilot> Pilots,
    IReadOnlyList<Mech> Mechs,
    IReadOnlyDictionary<string, ComponentDefinition> ComponentDefinitions,
    Position Position,
    Rules Rules);
