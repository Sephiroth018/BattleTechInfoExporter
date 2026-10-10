namespace BattleTechInfoExporter.Models;

/// <summary>What is about to land on a landing zone.</summary>
internal enum LandingZoneKind
{
    /// <summary>A dropship, which marks its whole footprint.</summary>
    Dropship,

    /// <summary>A lance's drop pods, which mark a square around each spawn point.</summary>
    DropPod
}
