namespace BattleTechInfoExporter.Models;

/// <summary>The kind of a refit step.</summary>
/// <remarks>The game orders a move as a removal followed by an installation of the same component.</remarks>
internal enum RefitChangeType
{
    /// <summary>Mounts a component taken from storage or removed from another location.</summary>
    InstallComponent,

    /// <summary>Takes a component off the mech, into storage or for installation elsewhere.</summary>
    RemoveComponent,

    /// <summary>Repairs a damaged component where it is mounted.</summary>
    RepairComponent,

    /// <summary>Changes a location's armor.</summary>
    ModifyArmor,

    /// <summary>Repairs a location's structure.</summary>
    RepairStructure
}
