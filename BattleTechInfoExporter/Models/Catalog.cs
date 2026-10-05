using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The root of <c>catalog.json</c>: every chassis, mech and component the game has loaded for the career, keyed
///     by id. It describes the game, not the career, so it only changes with the game's data.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Catalog(
    string ModVersion,
    ExportTrigger Trigger,
    IReadOnlyDictionary<string, ChassisDefinition> ChassisDefinitions,
    IReadOnlyDictionary<string, MechDefinition> MechDefinitions,
    ComponentDefinitions ComponentDefinitions) : ExportFile(ModVersion, null, Trigger);
