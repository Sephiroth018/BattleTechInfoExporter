using System;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A reference to a component's definition, with its type. Components are ordered by type, then by name, with
///     the id breaking ties.
/// </summary>
/// <param name="Id">The component definition's id from the game's data files.</param>
/// <param name="Type">
///     The game's component type, e.g. <c>Weapon</c> or <c>AmmunitionBox</c>, which names the component's group in
///     <see cref="Catalog.ComponentDefinitions" />; never <c>NotSet</c>, <c>Special</c> or <c>MechPart</c>, which the
///     game's enum also has.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ComponentReference(string Id, string Name, ComponentType Type)
    : Reference(Id, Name), IComparable<ComponentReference>
{
    public int CompareTo(ComponentReference? other)
    {
        if (other is null)
        {
            return 1;
        }

        var byType = Type.CompareTo(other.Type);
        return byType != 0 ? byType : base.CompareTo(other);
    }
}
