using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BattleTechInfoExporter.Models;

/// <summary>Whether a mech in the mech bay is free for use or held by the mech lab; written in camelCase.</summary>
[JsonConverter(typeof(StringEnumConverter), true)]
internal enum MechStatus
{
    Ready,

    /// <summary>Being readied from a stored chassis.</summary>
    Readying,

    /// <summary>Being repaired or refitted.</summary>
    InMaintenance
}
