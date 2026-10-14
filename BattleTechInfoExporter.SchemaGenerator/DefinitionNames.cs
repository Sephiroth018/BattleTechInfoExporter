using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema.Generation;

namespace BattleTechInfoExporter.SchemaGenerator;

/// <summary>
///     Names a type's schema definition: its own name, a generic one followed by <c>Of</c> and its type arguments
///     (<c>ByStoreOfStringList</c>), a list by its element.
/// </summary>
internal sealed class DefinitionNames : ISchemaNameGenerator
{
    public string Generate(Type type) => Of(type);

    internal static string Of(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var arguments = type.GetGenericArguments();
        var definition = type.GetGenericTypeDefinition();
        if (definition == typeof(Nullable<>))
        {
            return $"Nullable{Of(arguments[0])}";
        }

        if (definition == typeof(IReadOnlyList<>))
        {
            return $"{Of(arguments[0])}List";
        }

        var name = type.Name.Split('`')[0];
        if (type.IsGenericTypeDefinition)
        {
            return name;
        }

        // A generic named for its argument (HitTablesOf<T>) already ends in "Of".
        return $"{name}{(name.EndsWith("Of", StringComparison.Ordinal) ? string.Empty : "Of")}"
               + string.Join("And", arguments.Select(Of));
    }
}
