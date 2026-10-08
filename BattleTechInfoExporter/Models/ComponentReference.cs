using System;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A component's definition, like <see cref="DefinitionReference" />, with the type that tells which group of
///     <see cref="Catalog.ComponentDefinitions" /> holds it. Components compare by type first, then as every reference.
/// </summary>
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
