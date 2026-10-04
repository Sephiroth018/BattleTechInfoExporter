using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BattleTechInfoExporter.Models;

/// <summary>A pilot's temporary high or low spirits from an event; written in camelCase.</summary>
[JsonConverter(typeof(StringEnumConverter), true)]
internal enum SpiritsLevel
{
    High,
    Low
}
