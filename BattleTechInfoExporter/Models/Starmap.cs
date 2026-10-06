using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The root of <c>star-systems.json</c>: every star system on the starmap, keyed by id.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Starmap(
    string ModVersion,
    ExportTrigger Trigger,
    IReadOnlyDictionary<string, StarSystem> StarSystems) : ExportFile(ModVersion, null, Trigger);
