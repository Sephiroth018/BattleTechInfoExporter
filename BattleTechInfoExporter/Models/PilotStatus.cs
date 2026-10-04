using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BattleTechInfoExporter.Models;

/// <summary>Whether a pilot can be deployed, as the barracks shows it; written in camelCase.</summary>
[JsonConverter(typeof(StringEnumConverter), true)]
internal enum PilotStatus
{
    Ready,
    Injured,

    /// <summary>Out of action without injuries, because of an event.</summary>
    Unavailable
}
