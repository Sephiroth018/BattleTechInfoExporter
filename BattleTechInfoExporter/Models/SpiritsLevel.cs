namespace BattleTechInfoExporter.Models;

/// <summary>A pilot's spirits: normal, or temporarily high or low from an event.</summary>
internal enum SpiritsLevel
{
    /// <summary>Neither high nor low.</summary>
    Normal,

    /// <summary>High spirits, from an event.</summary>
    High,

    /// <summary>Low spirits, from an event.</summary>
    Low
}
