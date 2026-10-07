namespace BattleTechInfoExporter.Models;

/// <summary>How a unit could fire at a target from where it stands (the game's <c>FiringPreviewManager</c>).</summary>
internal enum FireAvailability
{
    /// <summary>The line of fire isn't blocked and the target is within the longest weapon's range.</summary>
    Direct,

    /// <summary>The line of fire is blocked, but the target is within an indirect fire weapon's range.</summary>
    Indirect,

    /// <summary>The line of fire isn't blocked, but the target is beyond the longest weapon's range.</summary>
    OutOfRange,

    /// <summary>Blocked, and no weapon can fire indirectly at it.</summary>
    None
}
