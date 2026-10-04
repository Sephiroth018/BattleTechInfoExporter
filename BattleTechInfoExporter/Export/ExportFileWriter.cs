using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace BattleTechInfoExporter.Export;

/// <summary>Writes export files into the mod's <c>exports</c> folder.</summary>
internal static class ExportFileWriter
{
    // Game enums keep the game's own values (e.g. IN_SYSTEM), which is what the UI shows. Dictionary keys are
    // game ids, kept as they are; CamelCasePropertyNamesContractResolver would camel-case them too.
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        Formatting = Formatting.Indented,
        ContractResolver = new InheritedFirstContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false }
        },
        NullValueHandling = NullValueHandling.Include,
        Converters = { new StringEnumConverter() }
    };

    private static readonly string ExportDirectory = Path.Combine(ModAssembly.Directory, "exports");

    /// <summary>Replaces the file in one step, so a tool reading it never sees a half-written file.</summary>
    internal static string Write(string fileName, object content)
    {
        Directory.CreateDirectory(ExportDirectory);
        var path = Path.Combine(ExportDirectory, fileName);
        var temporaryPath = path + ".tmp";
        File.WriteAllText(temporaryPath, JsonConvert.SerializeObject(content, SerializerSettings));
        if (File.Exists(path))
        {
            File.Replace(temporaryPath, path, null);
        }
        else
        {
            File.Move(temporaryPath, path);
        }

        return path;
    }

    /// <summary>
    ///     Writes inherited properties before a type's own, base type first, so e.g. every component definition
    ///     starts with its name; Newtonsoft.Json writes them the other way round.
    /// </summary>
    private sealed class InheritedFirstContractResolver : DefaultContractResolver
    {
        // OrderBy is stable, so the properties keep their declaration order within each type.
        protected override IList<JsonProperty> CreateProperties(
            Type type,
            MemberSerialization memberSerialization) =>
            base.CreateProperties(type, memberSerialization)
                .OrderBy(property => InheritanceDepth(property.DeclaringType))
                .ToList();

        private static int InheritanceDepth(Type? type) => type is null ? 0 : 1 + InheritanceDepth(type.BaseType);
    }
}
