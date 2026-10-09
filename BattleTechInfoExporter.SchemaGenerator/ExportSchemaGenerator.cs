using System;
using System.Collections.Generic;
using System.Linq;
using BattleTechInfoExporter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.NewtonsoftJson.Generation;

namespace BattleTechInfoExporter.SchemaGenerator;

/// <summary>
///     Generates an export file's schema with the serializer settings the mod writes the file with, and collects the
///     exported properties without a description.
/// </summary>
internal sealed class ExportSchemaGenerator
{
    private static readonly Type ModelType = typeof(ExportFile);

    private readonly NewtonsoftJsonSchemaGeneratorSettings _settings;
    private readonly SortedSet<string> _undescribedProperties = new(StringComparer.Ordinal);

    internal ExportSchemaGenerator()
    {
        var serializerSettings = ExportFile.SerializerSettings;
        _settings = new NewtonsoftJsonSchemaGeneratorSettings
        {
            SerializerSettings = serializerSettings,
            // Every type lists its inherited properties, as the file does, instead of an allOf of its base types.
            FlattenInheritanceHierarchy = true,
            GenerateEnumMappingDescription = false,
            // ModelDocumentation reads the descriptions instead, so they name what the JSON shows.
            UseXmlDocumentation = false,
            SchemaProcessors =
            {
                new AbstractRecordProcessor(),
                new ModelProcessor(
                    new ModelDocumentation(serializerSettings),
                    serializerSettings,
                    _undescribedProperties)
            }
        };
    }

    /// <summary>The exported properties without a description, as <c>Type.property</c>.</summary>
    internal IReadOnlyCollection<string> UndescribedProperties => _undescribedProperties;

    internal JsonSchema Generate(Type exportFileModel) =>
        NewtonsoftJsonSchemaGenerator.FromType(exportFileModel, _settings);

    private static bool IsModel(Type type) => type.Assembly == ModelType.Assembly;

    /// <summary>
    ///     An abstract record declared as a property's type is written as whichever derived record the value is, so it
    ///     becomes any of them.
    /// </summary>
    private sealed class AbstractRecordProcessor : ISchemaProcessor
    {
        public void Process(SchemaProcessorContext context)
        {
            var type = context.ContextualType.Type;
            if (!IsModel(type) || type is not { IsClass: true, IsAbstract: true })
            {
                return;
            }

            context.Schema.Properties.Clear();
            context.Schema.Type = JsonObjectType.None;
            context.Schema.AllowAdditionalProperties = true;
            // By name, so the schema doesn't change with the order reflection lists the types in.
            foreach (var derivedType in type.Assembly.GetTypes()
                         .Where(candidate => !candidate.IsAbstract && candidate.IsSubclassOf(type))
                         .OrderBy(derivedType => derivedType.Name, StringComparer.Ordinal))
            {
                context.Schema.AnyOf.Add(
                    new JsonSchema { Reference = context.Generator.Generate(derivedType, context.Resolver) });
            }
        }
    }

    /// <summary>
    ///     Describes the models and their properties, and requires every property, as the mod writes them all,
    ///     <c>null</c> included.
    /// </summary>
    private sealed class ModelProcessor : ISchemaProcessor
    {
        private readonly ModelDocumentation _documentation;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly ISet<string> _undescribedProperties;

        internal ModelProcessor(
            ModelDocumentation documentation,
            JsonSerializerSettings serializerSettings,
            ISet<string> undescribedProperties)
        {
            _documentation = documentation;
            _serializerSettings = serializerSettings;
            _undescribedProperties = undescribedProperties;
        }

        public void Process(SchemaProcessorContext context)
        {
            var type = context.ContextualType.Type;
            var schema = context.Schema;
            if (!IsModel(type))
            {
                // Game types have no description; NJsonSchema gives game enums an empty one.
                schema.Description = null;
                return;
            }

            schema.Description = _documentation.TypeDescription(type);
            if (_serializerSettings.ContractResolver!.ResolveContract(type) is not JsonObjectContract contract)
            {
                return;
            }

            foreach (var property in contract.Properties.Where(property =>
                         schema.Properties.ContainsKey(property.PropertyName!)))
            {
                var description = _documentation.PropertyDescription(type, property.UnderlyingName!);
                if (description is null)
                {
                    // Named by the record that declares it, where its description goes.
                    _undescribedProperties.Add($"{property.DeclaringType!.Name.Split('`')[0]}.{property.PropertyName}");
                }

                schema.Properties[property.PropertyName!].Description = description;
                schema.RequiredProperties.Add(property.PropertyName!);
            }
        }
    }
}
