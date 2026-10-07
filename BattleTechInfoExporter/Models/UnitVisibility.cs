namespace BattleTechInfoExporter.Models;

/// <summary>How much of a unit the player's HUD shows (the game's <c>VisibilityLevel</c>).</summary>
internal enum UnitVisibility
{
    /// <summary>In sight of the player's side, or one of its own units: everything the HUD can show.</summary>
    Full,

    /// <summary>In sight but hidden by ECM: no pilot, heat, stability or initiative, and it can't be targeted.</summary>
    Ghost,

    /// <summary>A sensor blip with the unit's kind and tonnage, or a turret's weight class.</summary>
    BlipMaximum,

    /// <summary>A sensor blip with the unit's kind.</summary>
    BlipType,

    /// <summary>A sensor blip with nothing but its position.</summary>
    BlipMinimum,

    /// <summary>Out of sensor range, where the player's side last detected it; not known to be destroyed.</summary>
    LastSeen
}
