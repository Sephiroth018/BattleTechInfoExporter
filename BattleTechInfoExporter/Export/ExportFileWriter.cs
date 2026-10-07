using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BattleTechInfoExporter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace BattleTechInfoExporter.Export;

/// <summary>Writes the export files in the mod's <c>exports</c> folder.</summary>
internal static class ExportFileWriter
{
    // Game enums keep the game's own values (e.g. IN_SYSTEM), which is what the UI shows; the mod's own enums are
    // camelCase. The first converter that can convert a type wins. Dictionary keys are game ids, kept as they are;
    // CamelCasePropertyNamesContractResolver would camel-case them too.
    private static readonly NamingStrategy PropertyNaming = new CamelCaseNamingStrategy
        { ProcessDictionaryKeys = false };

    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        Formatting = Formatting.None,
        ContractResolver = new InheritedFirstContractResolver { NamingStrategy = PropertyNaming },
        NullValueHandling = NullValueHandling.Include,
        Converters = { new OwnEnumConverter(), new StringEnumConverter() }
    };

    private static readonly string ExportDirectory = Path.Combine(ModAssembly.Directory, "exports");

    // The JSON last written per file name, without its ExportedAt and Trigger; empty after every game start, so
    // each file is written once per session.
    private static readonly Dictionary<string, string> LastWrittenContents = new();

    /// <summary>
    ///     Reads the string properties named by their record members (<c>nameof</c>) from the start of an existing
    ///     file, parsing only as far as the last of them: the files are large, and the header properties come first.
    ///     A property the file doesn't have is <c>null</c>.
    /// </summary>
    /// <returns><c>null</c> when the file doesn't exist.</returns>
    /// <exception cref="JsonException">The file is malformed.</exception>
    /// <exception cref="IOException">The file can't be read.</exception>
    internal static IReadOnlyDictionary<string, string?>? ReadHeader(string fileName, params string[] memberNames)
    {
        var path = FilePath(fileName);
        if (!File.Exists(path))
        {
            return null;
        }

        var membersByPropertyName = memberNames.ToDictionary(
            memberName => PropertyNaming.GetPropertyName(memberName, false),
            memberName => memberName,
            StringComparer.Ordinal);
        var header = memberNames.ToDictionary(memberName => memberName, _ => (string?)null, StringComparer.Ordinal);
        using var reader = new JsonTextReader(File.OpenText(path));
        if (!reader.Read() || reader.TokenType != JsonToken.StartObject)
        {
            throw new JsonReaderException($"{fileName} doesn't start with an object");
        }

        var remaining = memberNames.Length;
        while (remaining > 0 && reader.Read() && reader.TokenType == JsonToken.PropertyName)
        {
            if (reader.Value is string propertyName
                && membersByPropertyName.TryGetValue(propertyName, out var memberName))
            {
                header[memberName] = reader.ReadAsString();
                remaining--;
            }
            else
            {
                reader.Skip();
            }
        }

        return header;
    }

    private static string FilePath(string fileName) => Path.Combine(ExportDirectory, fileName);

    /// <summary>
    ///     Replaces the file in one step, so a tool reading it never sees a half-written file. Leaves it untouched
    ///     when its content, apart from <see cref="ExportFile.ExportedAt" /> and <see cref="ExportFile.Trigger" />,
    ///     is the same as the last one written this session, so tools watching it only see real changes.
    /// </summary>
    internal static void Write(string fileName, ExportFile content)
    {
        Directory.CreateDirectory(ExportDirectory);
        var path = FilePath(fileName);
        var comparedContent = JsonConvert.SerializeObject(
            content with { ExportedAt = null, Trigger = default },
            SerializerSettings);
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

    /// <summary>Deletes the file, if it exists, so tools see its data is gone.</summary>
    internal static void Delete(string fileName)
    {
        // Forgotten either way, so the next write of the file isn't skipped as unchanged.
        LastWrittenContents.Remove(fileName);
        var path = FilePath(fileName);
        if (!File.Exists(path))
        {
            return;
        }

        File.Delete(path);
        ModLog.Logger.Log($"Deleted {fileName} from {ExportDirectory}");
    }

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
