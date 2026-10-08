namespace BattleTechInfoExporter.Models;

/// <summary>A component in a mech's loadout, which is either part of the chassis or can be removed.</summary>
internal interface IFixedOrRemovable
{
    /// <summary>Part of the chassis; every mech of the chassis has it, and it can't be removed.</summary>
    bool IsFixed { get; }
}
