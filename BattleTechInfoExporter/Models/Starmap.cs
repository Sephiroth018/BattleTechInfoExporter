using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The root of <c>star-systems.json</c>: every star system on the starmap.</summary>
/// <param name="StarSystems">Every star system, keyed by the id of its active definition.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Starmap(
    IReadOnlyDictionary<string, StarSystem> StarSystems) : ExportFile
{
    internal const string FileName = "star-systems.json";

    protected override bool IsWrittenOnlyWhenChanged => true;
}
