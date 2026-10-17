namespace BattleTechInfoExporter.Models;

/// <summary>How much of a line of fire is blocked (the game's <c>LineOfFireLevel</c>).</summary>
internal enum LineOfFireBlocking
{
    /// <summary>Nothing in the way: direct fire without a line of fire penalty.</summary>
    Clear,

    /// <summary>
    ///     The game's obstructed line: direct fire is still possible, with the penalties of
    ///     <see cref="CombatRules.LineOfFire" />.
    /// </summary>
    PartiallyBlocked,

    /// <summary>No direct fire; a weapon that can fire indirectly still may.</summary>
    Blocked
}
