namespace BattleTechInfoExporter.Models;

/// <summary>How much of a line of fire is blocked (the game's <c>LineOfFireLevel</c>).</summary>
internal enum LineOfFireBlocking
{
    Clear,

    /// <summary>
    ///     The game's obstructed line: direct fire is still possible, with the penalties of <c>rules.json</c>'s
    ///     <see cref="LineOfFireRules" />.
    /// </summary>
    PartiallyBlocked,

    /// <summary>No direct fire; a weapon that can fire indirectly still may.</summary>
    Blocked
}
