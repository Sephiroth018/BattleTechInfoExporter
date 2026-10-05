using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BattleTechInfoExporter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace BattleTechInfoExporter.Export;

/// <summary>Writes export files into the mod's <c>exports</c> folder.</summary>
internal static class ExportFileWriter
{
    // Game enums keep the game's own values (e.g. IN_SYSTEM), which is what the UI shows; the mod's own enums are
    // camelCase. The first converter that can convert a type wins. Dictionary keys are game ids, kept as they are;
    // CamelCasePropertyNamesContractResolver would camel-case them too.
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        Formatting = Formatting.Indented,
        ContractResolver = new InheritedFirstContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false }
        },
        NullValueHandling = NullValueHandling.Include,
        Converters = { new OwnEnumConverter(), new StringEnumConverter() }
    };

    private static readonly string ExportDirectory = Path.Combine(ModAssembly.Directory, "exports");

    // The JSON last written per file name, without its ExportedAt; empty after every game start, so each file is
    // written once per session.
    private static readonly Dictionary<string, string> LastWrittenContents = new();

    /// <summary>
    ///     Replaces the file in one step, so a tool reading it never sees a half-written file. Leaves it untouched
    ///     when its content, apart from <see cref="ExportFile.ExportedAt" />, is the same as the last one written
    ///     this session, so tools watching it only see real changes.
    /// </summary>
    internal static void Write(string fileName, ExportFile content)
    {
        Directory.CreateDirectory(ExportDirectory);
        var path = Path.Combine(ExportDirectory, fileName);
        var comparedContent = JsonConvert.SerializeObject(content with { ExportedAt = null }, SerializerSettings);
        var fileExists = File.Exists(path);
        if (fileExists
            && LastWrittenContents.TryGetValue(fileName, out var lastWrittenContent)
            && lastWrittenContent == comparedContent)
        {
            ModLog.Logger.Log($"Left {fileName} unchanged ({content.Trigger})");
            return;
        }

        var json = JsonConvert.SerializeObject(content with { ExportedAt = DateTimeOffset.Now }, SerializerSettings);
        var temporaryPath = path + ".tmp";
        File.WriteAllText(temporaryPath, json);
        if (fileExists)
        {
            File.Replace(temporaryPath, path, null);
        }
        else
        {
            File.Move(temporaryPath, path);
        }

        // Only once the file is written, so a failed write is retried on the next export.
        LastWrittenContents[fileName] = comparedContent;
        ModLog.Logger.Log($"Exported {fileName} ({content.Trigger}) to {ExportDirectory}");
    }

    /// <summary>
    ///     Writes inherited properties before a type's own, base type first, so e.g. every component definition
    ///     starts with its name; Newtonsoft.Json writes them the other way round.
    /// </summary>
    // Newtonsoft.Json 10, the game's version, has no naming strategy for enums yet.
    private sealed class OwnEnumConverter : StringEnumConverter
    {
        internal OwnEnumConverter()
        {
            CamelCaseText = true;
        }

        public override bool CanConvert(Type objectType) =>
            base.CanConvert(objectType)
            && (Nullable.GetUnderlyingType(objectType) ?? objectType).Assembly == typeof(OwnEnumConverter).Assembly;
    }

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
