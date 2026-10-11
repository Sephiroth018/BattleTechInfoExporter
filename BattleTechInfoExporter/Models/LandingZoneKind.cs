namespace BattleTechInfoExporter.Models;

/// <summary>What is about to land on a landing zone.</summary>
internal enum LandingZoneKind
{
    /// <summary>A dropship, which marks its whole footprint.</summary>
    Dropship,

    /// <summary>A lance's drop pods, one per unit, each marking the hex it lands on.</summary>
    DropPod
}
