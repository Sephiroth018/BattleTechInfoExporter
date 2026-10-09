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
///     The descriptions of the models and their properties, from the mod's XML doc comments, as plain text that names
///     what the JSON shows: a <c>see cref</c> to a property becomes its JSON name, one to an enum value the value written.
/// </summary>
internal sealed partial class ModelDocumentation
{
    private readonly JsonSerializerSettings _serializerSettings;

    internal ModelDocumentation(JsonSerializerSettings serializerSettings)
    {
        _serializerSettings = serializerSettings;
    }

    internal string? TypeDescription(Type type) => Render(type.GetXmlDocsElement()?.Element("summary"));

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
                return Render(parameter);
            }
        }

        return Render(
            type.GetProperty(propertyName)?.GetXmlDocsElement()?.Element("summary")
            ?? type.GetInterfaces()
                .Select(implemented => implemented.GetProperty(propertyName)?.GetXmlDocsElement()?.Element("summary"))
                .FirstOrDefault(summary => summary is not null));
    }

    private string? Render(XElement? documentation)
    {
        if (documentation is null)
        {
            return null;
        }

        var text = Whitespace().Replace(string.Concat(documentation.Nodes().Select(RenderNode)), " ").Trim();
        return text.Length == 0 ? null : text;
    }

    private string RenderNode(XNode node) =>
        node switch
        {
            XText text => text.Value,
            XElement { Name.LocalName: "see" } see when see.Attribute("cref") is { } cref => JsonName(cref.Value),
            XElement { Name.LocalName: "see" } see when see.Attribute("langword") is { } word => word.Value,
            XElement { Name.LocalName: "paramref" } reference =>
                ExportFile.PropertyNaming.GetPropertyName((string)reference.Attribute("name")!, false),
            XElement element => string.Concat(element.Nodes().Select(RenderNode)),
            _ => string.Empty
        };

    /// <summary>
    ///     A cref (<c>P:Namespace.Type.Member</c>) as the JSON shows it: a property by its JSON name, an enum value as
    ///     written, a type by its name, which is also its schema definition's.
    /// </summary>
    private string JsonName(string cref)
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
            ('P', not null) when _serializerSettings.ContractResolver!.ResolveContract(type) is JsonObjectContract
                    contract
                => contract.Properties.FirstOrDefault(property => property.UnderlyingName == memberName)?.PropertyName
                   ?? memberName,
            _ => memberName
        };
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
