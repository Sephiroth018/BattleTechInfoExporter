using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BattleTechInfoExporter.Models;

/// <summary>A pilot's spirits: normal, or temporarily high or low from an event; written in camelCase.</summary>
[JsonConverter(typeof(StringEnumConverter), true)]
internal enum SpiritsLevel
{
    Normal,
    High,
    Low
}
