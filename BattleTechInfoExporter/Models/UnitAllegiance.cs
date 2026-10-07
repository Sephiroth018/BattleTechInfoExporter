namespace BattleTechInfoExporter.Models;

/// <summary>Whose side a unit in combat is on, seen from the player.</summary>
internal enum UnitAllegiance
{
    /// <summary>The player's own lance.</summary>
    Player,

    /// <summary>A team on the player's side, e.g. the employer's units.</summary>
    Ally,

    Enemy,

    /// <summary>A team neither side fights, until it turns hostile.</summary>
    Neutral
}
