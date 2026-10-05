namespace BattleTechInfoExporter.Models;

/// <summary>Whether a pilot can be deployed, as the barracks shows it.</summary>
internal enum PilotStatus
{
    Ready,
    Injured,

    /// <summary>Out of action without injuries, because of an event.</summary>
    Unavailable
}
