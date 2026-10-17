namespace BattleTechInfoExporter.Models;

/// <summary>Whether a pilot can be deployed, as the barracks shows it.</summary>
internal enum PilotStatus
{
    /// <summary>Can be deployed.</summary>
    Ready,

    /// <summary>Injured, healing in the med bay.</summary>
    Injured,

    /// <summary>Out of action without injuries, because of an event.</summary>
    Unavailable
}
