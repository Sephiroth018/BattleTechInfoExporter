using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using BattleTechInfoExporter.Models;
using Namotion.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BattleTechInfoExporter.SchemaGenerator;

/// <summary>
///     The descriptions of the models and their properties in one export file's schema, from the mod's XML doc
///     comments, as plain text that names what the JSON shows: a <c>see cref</c> to a property or type becomes where
///     the file holds it, or the other file and the path there, one to an enum value the value written.
/// </summary>
internal sealed partial class ModelDocumentation
{
    private readonly Type _fileModel;
    private readonly ExportFilePaths _paths;
    private readonly JsonSerializerSettings _serializerSettings;

    internal ModelDocumentation(JsonSerializerSettings serializerSettings, ExportFilePaths paths, Type fileModel)
    {
        _serializerSettings = serializerSettings;
        _paths = paths;
        _fileModel = fileModel;
    }

    internal string? TypeDescription(Type type) => Render(type.GetXmlDocsElement()?.Element("summary"), type);

    /// <summary>
    ///     The property's record parameter, from the type itself up through its base types, so a derived record's
    ///     parameter overrides its base's; failing that, a body property's summary, then that of an interface's property
    ///     it implements.
    /// </summary>
    internal string? PropertyDescription(Type type, string propertyName)
    {
        for (var declaringType = type; declaringType is not null; declaringType = declaringType.BaseType)
        {
            if (declaringType.GetXmlDocsElement()?.Elements("param")
                    .FirstOrDefault(candidate => (string?)candidate.Attribute("name") == propertyName) is { } parameter)
            {
                return Render(parameter, type);
            }
        }

        return Render(
            type.GetProperty(propertyName)?.GetXmlDocsElement()?.Element("summary")
            ?? type.GetInterfaces()
                .Select(implemented => implemented.GetProperty(propertyName)?.GetXmlDocsElement()?.Element("summary"))
                .FirstOrDefault(summary => summary is not null),
            type);
    }

    // The documented type is the model whose schema the description goes into.
    private string? Render(XElement? documentation, Type documentedType)
    {
        if (documentation is null)
        {
            return null;
        }

        var text = Whitespace()
            .Replace(string.Concat(documentation.Nodes().Select(node => RenderNode(node, documentedType))), " ")
            .Trim();
        return text.Length == 0 ? null : text;
    }

    private string RenderNode(XNode node, Type documentedType) =>
        node switch
        {
            XText text => text.Value,
            XElement { Name.LocalName: "see" } see when see.Attribute("cref") is { } cref =>
                Target(cref.Value, documentedType),
            XElement { Name.LocalName: "see" } see when see.Attribute("langword") is { } word => word.Value,
            XElement { Name.LocalName: "paramref" } reference =>
                ExportFile.PropertyNaming.GetPropertyName((string)reference.Attribute("name")!, false),
            XElement element => string.Concat(element.Nodes().Select(child => RenderNode(child, documentedType))),
            _ => string.Empty
        };

    /// <summary>
    ///     A cref (<c>P:Namespace.Type.Member</c>) as the JSON shows it: an enum value as written; a property by its
    ///     JSON name alone on the documented type, as the type itself or one it derives from has it, or on the root; a
    ///     property or type elsewhere in this file by its owner's definition and JSON name (<c>Mech.id</c>); one in
    ///     another file by that file and its path there (<c>rules.json moraleLevels</c>); anything else by its name.
    /// </summary>
    private string Target(string cref, Type documentedType)
    {
        var kind = cref[0];
        var name = cref[2..].Split('(')[0];
        var typeName = kind == 'T' ? name : name[..name.LastIndexOf('.')];
        var memberName = name[(name.LastIndexOf('.') + 1)..].Split('`')[0];
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(typeName))
            .FirstOrDefault(type => type is not null);
        return (kind, type) switch
        {
            ('F', { IsEnum: true }) =>
                JsonConvert.SerializeObject(Enum.Parse(type, memberName), _serializerSettings).Trim('"'),
            ('P', not null) when JsonNameOf(type, memberName) is { } jsonName =>
                type.IsAssignableFrom(documentedType) ? jsonName : Location(type, jsonName),
            ('T', not null) => Location(type, null),
            _ => memberName
        };
    }

    private string? JsonNameOf(Type owner, string propertyName) =>
        _serializerSettings.ContractResolver!.ResolveContract(owner) is JsonObjectContract contract
            ? contract.Properties.FirstOrDefault(property => property.UnderlyingName == propertyName)?.PropertyName
            : null;

    // A property, or a type where propertyName is null.
    private string Location(Type owner, string? propertyName)
    {
        if (_paths.PathIn(_fileModel, owner) is { } path)
        {
            return path.Length == 0
                ? propertyName ?? "the file's root"
                : ExportFilePaths.Join(DefinitionNames.Of(owner), propertyName);
        }

        if (_paths.ShortestPath(owner) is var (fileName, otherPath))
        {
            return $"{fileName} {ExportFilePaths.Join(otherPath, propertyName)}".TrimEnd();
        }

        // An abstract record no property declares is only ever written as one of its derived records.
        if (ExportSchemaGenerator.IsModel(owner) && owner.IsAbstract)
        {
            var locations = ExportSchemaGenerator.DerivedRecordsOf(owner)
                .Select(derivedType => Location(derivedType, propertyName))
                .ToList();
            return locations.Count > 1
                ? $"{string.Join(", ", locations[..^1])} and {locations[^1]}"
                : string.Concat(locations);
        }

        return propertyName ?? DefinitionNames.Of(owner);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
