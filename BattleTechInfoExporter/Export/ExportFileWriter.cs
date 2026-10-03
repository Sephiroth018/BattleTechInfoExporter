using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace BattleTechInfoExporter.Export;

/// <summary>Writes export files into the mod's <c>exports</c> folder.</summary>
internal static class ExportFileWriter
{
    // Game enums keep the game's own values (e.g. IN_SYSTEM), which is what the UI shows.
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        Formatting = Formatting.Indented,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
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
            File.Replace(temporaryPath, path, null);
        else
            File.Move(temporaryPath, path);

        return path;
    }
}
