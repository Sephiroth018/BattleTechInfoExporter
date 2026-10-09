using System;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A component's definition, like <see cref="DefinitionReference" />, with the type that tells which group of
///     <see cref="Catalog.ComponentDefinitions" /> holds it. Components compare by type first, then as every reference.
/// </summary>
/// <param name="Type">
///     The game's component type, e.g. <c>Weapon</c> or <c>AmmunitionBox</c>, which names the component's group in
///     <see cref="Catalog.ComponentDefinitions" />.
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
