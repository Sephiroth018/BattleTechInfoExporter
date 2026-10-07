using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The root of <c>star-systems.json</c>: every star system on the starmap, keyed by the id of its active
///     definition in the catalog (<see cref="StarSystemDefinition" />).
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Starmap(
    string ModVersion,
    ExportTrigger Trigger,
    IReadOnlyDictionary<string, StarSystem> StarSystems) : ExportFile(ModVersion, null, Trigger);
