using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BattleTechInfoExporter.Models;

/// <summary>The game's pilot types (as SimGameState.GetPilotTypeColor tells them apart); written in camelCase.</summary>
[JsonConverter(typeof(StringEnumConverter), true)]
internal enum PilotType
{
    Commander,

    /// <summary>A Kickstarter backer's pilot.</summary>
    Vanguard,
    Ronin,
    Regular
}
