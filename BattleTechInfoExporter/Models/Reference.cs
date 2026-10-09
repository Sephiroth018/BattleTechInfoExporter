using System;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     Something the export refers to by its id instead of repeating it, with the name the UI shows. References
///     compare by name, with the id breaking ties, so every list of entries that refer to a definition, which the game
///     keeps in no meaningful order, is ordered the same way.
/// </summary>
/// <param name="Id">
///     The id it's referred to by: for a definition its id from the game's data files, for a mech in the mech bay
///     or a pilot the game's own id of it.
/// </param>
/// <param name="Name">The name the UI shows.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record Reference(string Id, string Name) : IComparable<Reference>
{
    public int CompareTo(Reference? other)
    {
        if (other is null)
        {
            return 1;
        }

        var byName = string.CompareOrdinal(Name, other.Name);
        return byName != 0 ? byName : string.CompareOrdinal(Id, other.Id);
    }
}
