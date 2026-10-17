namespace BattleTechInfoExporter.Models;

/// <summary>How much of a unit the player's HUD shows (the game's <see cref="BattleTech.VisibilityLevel" />).</summary>
internal enum UnitVisibility
{
    /// <summary>
    ///     In sight of the player's side, allied units' sensors included, or one of its own units: everything the HUD
    ///     can show. An enemy in sight but hidden by ECM counts too, though the HUD hides its pilot, heat, stability
    ///     and initiative. A destroyed unit stays in full view.
    /// </summary>
    Full,

    /// <summary>A sensor blip with the unit's kind and tonnage, or a turret's weight class.</summary>
    BlipMaximum,

    /// <summary>A sensor blip with the unit's kind.</summary>
    BlipType,

    /// <summary>A sensor blip with nothing but its position.</summary>
    BlipMinimum,

    /// <summary>Out of sensor range, where the player's side last detected it; not known to be destroyed.</summary>
    LastSeen
}
