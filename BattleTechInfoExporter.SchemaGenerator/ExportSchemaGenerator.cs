using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BattleTechInfoExporter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.NewtonsoftJson.Generation;

namespace BattleTechInfoExporter.SchemaGenerator;

/// <summary>
///     Generates an export file's schema with the serializer settings the mod writes the file with, and collects the
///     definitions and exported properties without a description.
/// </summary>
internal sealed class ExportSchemaGenerator
{
    private static readonly Type ModelType = typeof(ExportFile);
    private static readonly Type[] ModelTypes = ModelType.Assembly.GetTypes();

    private readonly ExportFilePaths _paths;
    private readonly JsonSerializerSettings _serializerSettings = ExportFile.SerializerSettings;
    private readonly SortedSet<string> _undescribed = new(StringComparer.Ordinal);

    internal ExportSchemaGenerator()
    {
        _paths = new ExportFilePaths(_serializerSettings);
    }

    /// <summary>
    ///     The definitions without a description, as <c>Type</c>, and the exported properties without one, as
    ///     <c>Type.property</c>.
    /// </summary>
    internal IReadOnlyCollection<string> Undescribed => _undescribed;

    internal JsonSchema Generate(Type exportFileModel) =>
        NewtonsoftJsonSchemaGenerator.FromType(
            exportFileModel,
            new NewtonsoftJsonSchemaGeneratorSettings
            {
                SerializerSettings = _serializerSettings,
                SchemaNameGenerator = new DefinitionNames(),
                // Every type lists its inherited properties, as the file does, instead of an allOf of its base types.
                FlattenInheritanceHierarchy = true,
                GenerateEnumMappingDescription = false,
                // ModelDocumentation reads the descriptions instead, so they name what the JSON shows.
                UseXmlDocumentation = false,
                SchemaProcessors =
                {
                    new EnumAliasProcessor(),
                    new AbstractRecordProcessor(),
                    new ShapeValueProcessor(_serializerSettings),
                    new GameEnumProcessor(_undescribed),
                    new ModelProcessor(
                        new ModelDocumentation(_serializerSettings, _paths, exportFileModel),
                        _serializerSettings,
                        _undescribed)
                }
            });

    internal static bool IsModel(Type type) => type.Assembly == ModelType.Assembly;

    /// <summary>
    ///     The records an abstract record is written as, by name, so the schema doesn't depend on reflection's order.
    /// </summary>
    internal static IEnumerable<Type> DerivedRecordsOf(Type abstractRecord) =>
        ModelTypes.Where(candidate => !candidate.IsAbstract && candidate.IsSubclassOf(abstractRecord))
            .OrderBy(derivedType => derivedType.Name, StringComparer.Ordinal);

    // By the value as written: the game's enums by name, the mod's in camelCase, while the names are the members'.
    private static void KeepEnumValues(JsonSchema enumSchema, Func<string, bool> keep)
    {
        var kept = enumSchema.Enumeration.Zip(enumSchema.EnumerationNames)
            .Where(member => keep((string)member.First!))
            .ToList();
        enumSchema.Enumeration.Clear();
        enumSchema.EnumerationNames.Clear();
        foreach (var (value, name) in kept)
        {
            enumSchema.Enumeration.Add(value);
            enumSchema.EnumerationNames.Add(name);
        }
    }

    /// <summary>
    ///     An enum alias (the game's <c>ALLIED = HONORED</c>) is written under its value's name, so the schema lists
    ///     that name once and the alias not at all.
    /// </summary>
    private sealed class EnumAliasProcessor : ISchemaProcessor
    {
        public void Process(SchemaProcessorContext context)
        {
            var schema = context.Schema;
            if (!context.ContextualType.Type.IsEnum || schema.EnumerationNames.Count != schema.Enumeration.Count)
            {
                return;
            }

            var members = schema.Enumeration.Zip(schema.EnumerationNames, (value, name) => (Value: value, Name: name))
                .GroupBy(member => member.Value)
                .Select(sameValue => sameValue.First())
                .ToList();
            schema.Enumeration.Clear();
            schema.EnumerationNames.Clear();
            foreach (var (value, name) in members)
            {
                schema.Enumeration.Add(value);
                schema.EnumerationNames.Add(name);
            }
        }
    }

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
            foreach (var derivedType in DerivedRecordsOf(type))
            {
                context.Schema.AnyOf.Add(
                    new JsonSchema { Reference = context.Generator.Generate(derivedType, context.Resolver) });
            }
        }
    }

    /// <summary>
    ///     A switch property, an enum property of an abstract record that tells its derived records apart, lists in
    ///     each derived record's schema only that record's values, and its enum only the values some derived record
    ///     has.
    /// </summary>
    private sealed class ShapeValueProcessor : ISchemaProcessor
    {
        // Lazy, so a model the detection rejects fails with its own message rather than a type initializer's.
        private static readonly Lazy<IReadOnlyList<PropertyInfo>> SwitchProperties = new(() => ModelTypes
            .Where(model => model is { IsClass: true, IsAbstract: true })
            .SelectMany(record =>
                record.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            .Where(property => property.PropertyType.IsEnum && IsSwitch(property))
            .ToList());

        private readonly JsonSerializerSettings _serializerSettings;

        internal ShapeValueProcessor(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public void Process(SchemaProcessorContext context)
        {
            var type = context.ContextualType.Type;
            var schema = context.Schema;
            if (type.IsEnum && IsOnlySwitch(type))
            {
                var written = SwitchProperties.Value.Where(property => property.PropertyType == type)
                    .SelectMany(property => DerivedRecordsOf(property.DeclaringType!)
                        .SelectMany(shape => ShapeValues(shape, property)))
                    .ToHashSet();
                KeepEnumValues(schema, written.Contains);
                return;
            }

            if (type.IsAbstract
                || _serializerSettings.ContractResolver!.ResolveContract(type) is not JsonObjectContract contract)
            {
                return;
            }

            foreach (var switchProperty in SwitchProperties.Value.Where(property =>
                         type.IsSubclassOf(property.DeclaringType!)))
            {
                var schemaProperty =
                    schema.Properties[contract.Properties.Single(property =>
                        property.UnderlyingName == switchProperty.Name).PropertyName!];
                // Draft-04 ignores keywords next to $ref, so the enum's definition moves into an allOf.
                schemaProperty.AllOf.Add(new JsonSchema
                {
                    Reference = schemaProperty.Reference
                                ?? throw new InvalidOperationException(
                                    $"{type.Name}.{switchProperty.Name} isn't a plain reference to its enum")
                });
                schemaProperty.Reference = null;
                foreach (var value in ShapeValues(type, switchProperty))
                {
                    schemaProperty.Enumeration.Add(value);
                }
            }
        }

        // Whether the enum is the type of switch properties and of no other model property, also not inside a nullable,
        // list or dictionary, so narrowing it to their values loses no other property's.
        private static bool IsOnlySwitch(Type enumType)
        {
            var properties = ModelTypes
                .SelectMany(model =>
                    model.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                .Where(property => Mentions(property.PropertyType, enumType))
                .ToList();
            return properties.Count > 0 && properties.All(SwitchProperties.Value.Contains);
        }

        private static bool Mentions(Type type, Type enumType) =>
            type == enumType
            || (type.IsGenericType && type.GetGenericArguments().Any(argument => Mentions(argument, enumType)))
            || (type.GetElementType() is { } element && Mentions(element, enumType));

        // A property the abstract record's constructor takes, and each derived record states the values of: by its
        // base constructor call or a static set of them. Where only some derived records do, it fails, since those
        // that take it as an argument would say nothing of the values they are written with.
        private static bool IsSwitch(PropertyInfo property)
        {
            var record = property.DeclaringType!;
            if (!record.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Any(constructor => TakesAsArgument(constructor, property)))
            {
                return false;
            }

            var statesValues = DerivedRecordsOf(record)
                .Select(shape => ValueSetOf(shape, property) is not null
                                 || !TakesAsArgument(ConstructorOf(shape), property))
                .Distinct()
                .ToList();
            return statesValues switch
            {
                [var all] => all,
                [] => false,
                _ => throw new InvalidOperationException(
                    $"Only some records derived from {record.Name} state their {property.Name} values; give the "
                    + "others a static set of them")
            };
        }

        private static bool TakesAsArgument(ConstructorInfo constructor, PropertyInfo property) =>
            constructor.GetParameters()
                .Any(parameter => parameter.Name == property.Name && parameter.ParameterType == property.PropertyType);

        // The constructor the mod builds the record with, whatever its access, not the copy constructor `with` uses.
        private static ConstructorInfo ConstructorOf(Type shape)
        {
            var constructors = shape
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(constructor => constructor.GetParameters() is not [{ ParameterType: var only }] || only != shape)
                .ToList();
            return constructors.Count == 1
                ? constructors[0]
                : throw new InvalidOperationException(
                    $"{shape.Name} has {constructors.Count} constructors; its switch values can't be told from one");
        }

        private static PropertyInfo? ValueSetOf(Type shape, PropertyInfo switchProperty)
        {
            var valueSetType = typeof(IEnumerable<>).MakeGenericType(switchProperty.PropertyType);
            return shape.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(property => valueSetType.IsAssignableFrom(property.PropertyType));
        }

        // The values of the switch property a derived record is written with, as the JSON writes them: those its
        // static set of them lists, or else the one its base constructor call fixes. That value exists only in the
        // compiled call, so it's read by reflection from an instance built with default arguments.
        private IEnumerable<string> ShapeValues(Type shape, PropertyInfo switchProperty)
        {
            IEnumerable values;
            if (ValueSetOf(shape, switchProperty) is { } valueSet)
            {
                values = (IEnumerable)valueSet.GetValue(null)!;
            }
            else
            {
                var constructor = ConstructorOf(shape);
                var instance = constructor.Invoke(constructor.GetParameters()
                    .Select(parameter =>
                        parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null)
                    .ToArray());
                values = new[] { switchProperty.GetValue(instance) };
            }

            var serializer = JsonSerializer.Create(_serializerSettings);
            return values.Cast<object>().Select(value => JToken.FromObject(value, serializer).Value<string>()!);
        }
    }

    /// <summary>Describes the game's enums, and lists only the values the mod writes.</summary>
    private sealed class GameEnumProcessor : ISchemaProcessor
    {
        private readonly ISet<string> _undescribed;

        internal GameEnumProcessor(ISet<string> undescribed)
        {
            _undescribed = undescribed;
        }

        public void Process(SchemaProcessorContext context)
        {
            var type = context.ContextualType.Type;
            if (!type.IsEnum || IsModel(type))
            {
                return;
            }

            if (GameEnums.Of(type) is not { } gameEnum)
            {
                _undescribed.Add(type.Name);
                return;
            }

            context.Schema.Description = gameEnum.Description;
            KeepEnumValues(context.Schema, name => !gameEnum.NeverWritten.Contains(name));
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
        private readonly ISet<string> _undescribed;

        internal ModelProcessor(
            ModelDocumentation documentation,
            JsonSerializerSettings serializerSettings,
            ISet<string> undescribed)
        {
            _documentation = documentation;
            _serializerSettings = serializerSettings;
            _undescribed = undescribed;
        }

        public void Process(SchemaProcessorContext context)
        {
            var type = context.ContextualType.Type;
            var schema = context.Schema;
            if (!IsModel(type))
            {
                return;
            }

            schema.Description = _documentation.TypeDescription(type);
            if (schema.Description is null)
            {
                _undescribed.Add(type.Name.Split('`')[0]);
            }

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
                    _undescribed.Add($"{property.DeclaringType!.Name.Split('`')[0]}.{property.PropertyName}");
                }

                schema.Properties[property.PropertyName!].Description = description;
                schema.RequiredProperties.Add(property.PropertyName!);
            }
        }
    }
}
