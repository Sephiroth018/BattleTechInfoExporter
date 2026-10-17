using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BattleTechInfoExporter.SchemaGenerator;

/// <summary>
///     The JSON path from each export file's root to every model type and enum in it, one in the fewest steps where a
///     type occurs in several places: properties joined by <c>.</c>, a list's entries as <c>[]</c>, a dictionary's values
///     as
///     <c>*</c>. The root's own path is empty.
/// </summary>
internal sealed class ExportFilePaths
{
    private readonly IContractResolver _contractResolver;
    private readonly IReadOnlyList<(Type Model, string FileName, IReadOnlyDictionary<Type, string> Paths)> _files;

    internal ExportFilePaths(JsonSerializerSettings serializerSettings)
    {
        _contractResolver = serializerSettings.ContractResolver!;
        _files = ExportFileModels.All
            .Select(file => (file.Model, file.FileName, (IReadOnlyDictionary<Type, string>)PathsFrom(file.Model)))
            .ToList();
    }

    /// <summary>The type's path in the export file of <paramref name="fileModel" />; <c>null</c> where it has none.</summary>
    internal string? PathIn(Type fileModel, Type type) =>
        _files.Single(file => file.Model == fileModel).Paths.TryGetValue(type, out var path) ? path : null;

    /// <summary>
    ///     The type's home: the export file whose root keys it by id (<c>star-systems.json starSystems.*</c>), or else
    ///     the one holding it in the fewest steps, the first in <see cref="ExportFileModels.All" /> on a tie;
    ///     <c>null</c> when no file holds it.
    /// </summary>
    internal (string FileName, string Path)? HomePath(Type type) =>
        _files.Where(file => file.Paths.ContainsKey(type))
            .Select(file => ((string FileName, string Path)?)(file.FileName, file.Paths[type]))
            .MinBy(file => (IsKeyedAtRoot(file!.Value.Path) ? 0 : 1, StepsOf(file.Value.Path)));

    // A value of a dictionary on the root, such as rootProperty.*.
    private static bool IsKeyedAtRoot(string path) =>
        path.EndsWith(".*", StringComparison.Ordinal) && StepsOf(path) == 1;

    private static int StepsOf(string path) => path.Count(character => character is '.' or '[');

    // Breadth first, in the order the file writes its properties, so the first path found to a type is a shortest one.
    private Dictionary<Type, string> PathsFrom(Type fileModel)
    {
        var paths = new Dictionary<Type, string> { [fileModel] = string.Empty };
        var pending = new Queue<Type>([fileModel]);
        while (pending.Count > 0)
        {
            var type = pending.Dequeue();
            if (_contractResolver.ResolveContract(type) is not JsonObjectContract contract)
            {
                continue;
            }

            foreach (var property in contract.Properties.Where(property => !property.Ignored))
            {
                var (valueType, path) = Unwrap(property.PropertyType!, Join(paths[type], property.PropertyName!));
                foreach (var shape in ShapesOf(valueType).Where(shape => !paths.ContainsKey(shape)))
                {
                    paths[shape] = path;
                    pending.Enqueue(shape);
                }
            }
        }

        return paths;
    }

    // The type a value finally holds, through nullables, lists and dictionaries.
    private (Type Type, string Path) Unwrap(Type type, string path)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlying)
        {
            return (underlying, path);
        }

        return _contractResolver.ResolveContract(type) switch
        {
            JsonDictionaryContract dictionary => Unwrap(dictionary.DictionaryValueType!, $"{path}.*"),
            JsonArrayContract list => Unwrap(list.CollectionItemType!, $"{path}[]"),
            _ => (type, path)
        };
    }

    // An abstract record is written as whichever derived record the value is.
    private static IEnumerable<Type> ShapesOf(Type type) =>
        ExportSchemaGenerator.IsModel(type) && type is { IsClass: true, IsAbstract: true }
            ? [type, .. ExportSchemaGenerator.DerivedRecordsOf(type)]
            : [type];

    /// <summary>The path to a property, or the path itself where <paramref name="propertyName" /> is <c>null</c>.</summary>
    internal static string Join(string path, string? propertyName) =>
        propertyName is null ? path
        : path.Length == 0 ? propertyName
        : $"{path}.{propertyName}";
}
