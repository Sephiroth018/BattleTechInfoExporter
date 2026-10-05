namespace BattleTechInfoExporter.Models;

/// <summary>The game's pilot types, as SimGameState.GetPilotTypeColor tells them apart.</summary>
internal enum PilotType
{
    Commander,

    /// <summary>A Kickstarter backer's pilot.</summary>
    Vanguard,
    Ronin,
    Regular
}
