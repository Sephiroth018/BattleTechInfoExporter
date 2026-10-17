namespace BattleTechInfoExporter.Models;

/// <summary>The game's pilot types.</summary>
internal enum PilotType
{
    /// <summary>The player's commander.</summary>
    Commander,

    /// <summary>A Kickstarter backer's pilot.</summary>
    Vanguard,

    /// <summary>A premade pilot with its own name, portrait and backstory, e.g. a starting pilot.</summary>
    Ronin,

    /// <summary>Any other pilot, e.g. one the game generated for a hiring hall.</summary>
    Regular
}
