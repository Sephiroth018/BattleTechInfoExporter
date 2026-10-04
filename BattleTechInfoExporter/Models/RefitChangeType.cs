using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BattleTechInfoExporter.Models;

/// <summary>The kind of a refit step; written in camelCase.</summary>
/// <remarks>The game orders a move as a removal followed by an installation of the same component.</remarks>
[JsonConverter(typeof(StringEnumConverter), true)]
internal enum RefitChangeType
{
    /// <summary>Mounts a component taken from storage or removed from another location.</summary>
    InstallComponent,

    /// <summary>Takes a component off the mech, into storage or for installation elsewhere.</summary>
    RemoveComponent,

    RepairComponent,
    ModifyArmor,
    RepairStructure
}
