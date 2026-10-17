namespace BattleTechInfoExporter.Models;

/// <summary>The kind of a unit in combat, which tells the catalog group of its definition.</summary>
internal enum UnitKind
{
    /// <summary>A mech, the player's, an ally's or an enemy's.</summary>
    Mech,

    /// <summary>A ground unit with fixed armor and loadout.</summary>
    Vehicle,

    /// <summary>A fixed emplacement with a single location.</summary>
    Turret
}
