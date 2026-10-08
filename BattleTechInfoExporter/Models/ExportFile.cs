using System;
using System.Collections.Generic;
using System.Linq;
using BattleTechInfoExporter.Export;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The root of an export file: the properties every file starts with, and how the file is written. The header
///     properties are set only by <see cref="Write" />.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record ExportFile
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

    // The comparable JSON (ComparableContent) last written per file name; empty after every game start, so each
    // file is written once per session.
    private static readonly Dictionary<string, string> LastWrittenContents = new();

    public string ModVersion { get; private init; } = string.Empty;

    /// <summary>When the file was last changed.</summary>
    public DateTimeOffset ExportedAt { get; private init; }

    /// <summary>What caused the export that last changed the file.</summary>
    public ExportTrigger Trigger { get; private init; }

    /// <summary>
    ///     Replaces the file in one step (<see cref="ExportFileWriter.Replace" />), stamped with the header. Leaves it
    ///     untouched when its <see cref="ComparableContent" /> is the same as the last
    ///     one written this session, so tools watching it only see real changes.
    /// </summary>
    internal void Write(string fileName, ExportTrigger trigger)
    {
        var comparableContent = JsonConvert.SerializeObject(ComparableContent(), SerializerSettings);
        if (ExportFileWriter.Exists(fileName)
            && LastWrittenContents.TryGetValue(fileName, out var lastWrittenContent)
            && lastWrittenContent == comparableContent)
        {
            ModLog.Logger.Log($"Left {fileName} unchanged ({trigger})");
            return;
        }

        ExportFileWriter.Replace(
            fileName,
            JsonConvert.SerializeObject(
                this with { ModVersion = ModAssembly.Version, ExportedAt = DateTimeOffset.Now, Trigger = trigger },
                SerializerSettings));
        // Only once the file is written, so a failed write is retried on the next export.
        LastWrittenContents[fileName] = comparableContent;
        ModLog.Logger.Log($"Exported {fileName} ({trigger}) to {ExportFileWriter.ExportDirectory}");
    }

    /// <summary>Deletes the file, if it exists, so tools see its data is gone.</summary>
    internal static void Delete(string fileName)
    {
        // Forgotten either way, so the next write of the file isn't skipped as unchanged.
        LastWrittenContents.Remove(fileName);
        ExportFileWriter.Delete(fileName);
    }

    /// <summary>
    ///     The content <see cref="Write" /> compares: what the file says apart from when and why it was written.
    ///     Values that change without anything else changing, which aren't worth a write of their own, are cleared too.
    /// </summary>
    protected virtual ExportFile ComparableContent() => this with { ExportedAt = default, Trigger = default };

    /// <summary>
    ///     Reads the string properties named by their record members (<c>nameof</c>) from the header of an existing
    ///     file (<see cref="ExportFileWriter.ReadHeader" />); a property the file doesn't have is <c>null</c>.
    /// </summary>
    /// <returns><c>null</c> when the file doesn't exist.</returns>
    /// <exception cref="JsonException">The file is malformed.</exception>
    /// <exception cref="System.IO.IOException">The file can't be read.</exception>
    protected static IReadOnlyDictionary<string, string?>? ReadHeader(string fileName, params string[] memberNames)
    {
        return ExportFileWriter.ReadHeader(fileName, memberNames.Select(PropertyNameOf).ToList()) is { } header
            ? memberNames.ToDictionary(
                memberName => memberName,
                memberName => header[PropertyNameOf(memberName)],
                StringComparer.Ordinal)
            : null;

        static string PropertyNameOf(string memberName) => PropertyNaming.GetPropertyName(memberName, false);
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
    ///     Writes inherited properties before a type's own, base type first, so e.g. every file starts with its header
    ///     and every component definition with its name; Newtonsoft.Json writes them the other way round.
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
