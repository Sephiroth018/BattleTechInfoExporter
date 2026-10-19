namespace BattleTechInfoExporter.Models;

/// <summary>The side of a target an attack hits (the game's <see cref="BattleTech.AttackDirection" />).</summary>
internal enum AttackSide
{
    /// <summary>The target's front.</summary>
    Front,

    /// <summary>The target's left side.</summary>
    Left,

    /// <summary>The target's right side.</summary>
    Right,

    /// <summary>The target's rear.</summary>
    Rear,

    /// <summary>A prone target, which an attack hits anywhere.</summary>
    Prone,

    /// <summary>A turret, which has a single location.</summary>
    Turret
}
