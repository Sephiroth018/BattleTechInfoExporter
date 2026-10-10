using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace BattleTechInfoExporter.Export;

/// <summary>The files in the mod's <c>exports</c> folder, by file name.</summary>
internal static class ExportFileWriter
{
    internal static readonly string ExportDirectory = Path.Combine(ModAssembly.Directory, "exports");

    internal static bool Exists(string fileName) => File.Exists(FilePath(fileName));

    /// <summary>
    ///     Reads the string properties from the start of an existing file, parsing only as far as the last of them:
    ///     the files are large, and the header properties come first. A property the file doesn't have is
    ///     <c>null</c>.
    /// </summary>
    /// <returns>The values by property name; <c>null</c> when the file doesn't exist.</returns>
    /// <exception cref="JsonException">The file is malformed.</exception>
    /// <exception cref="IOException">The file can't be read.</exception>
    internal static IReadOnlyDictionary<string, string?>? ReadHeader(string fileName,
        IReadOnlyList<string> propertyNames)
    {
        var path = FilePath(fileName);
        if (!File.Exists(path))
        {
            return null;
        }

        var header = propertyNames.ToDictionary(
            propertyName => propertyName,
            _ => (string?)null,
            StringComparer.Ordinal);
        using var reader = new JsonTextReader(File.OpenText(path));
        if (!reader.Read() || reader.TokenType != JsonToken.StartObject)
        {
            throw new JsonReaderException($"{fileName} doesn't start with an object");
        }

        var remaining = propertyNames.Count;
        while (remaining > 0 && reader.Read() && reader.TokenType == JsonToken.PropertyName)
        {
            if (reader.Value is string propertyName && header.ContainsKey(propertyName))
            {
                header[propertyName] = reader.ReadAsString();
                remaining--;
            }
            else
            {
                reader.Skip();
            }
        }

        return header;
    }

    /// <summary>Replaces the file in one step, so a tool reading it never sees a half-written file.</summary>
    internal static void Replace(string fileName, string content)
    {
        Directory.CreateDirectory(ExportDirectory);
        var path = FilePath(fileName);
        var temporaryPath = path + ".tmp";
        File.WriteAllText(temporaryPath, content);
        if (File.Exists(path))
        {
            File.Replace(temporaryPath, path, null);
        }
        else
        {
            File.Move(temporaryPath, path);
        }
    }

    internal static void Delete(string fileName)
    {
        var path = FilePath(fileName);
        if (!File.Exists(path))
        {
            return;
        }

        File.Delete(path);
        ModLog.Logger.Log($"Deleted {fileName} from {ExportDirectory}");
    }

    /// <summary>
    ///     Deletes every file in the folder but those named, e.g. the files of an earlier mod version, so a tool never
    ///     reads one that's no longer kept current.
    /// </summary>
    /// <exception cref="IOException">A file can't be deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">A file can't be deleted.</exception>
    internal static void DeleteAllExcept(IEnumerable<string> fileNames)
    {
        if (!Directory.Exists(ExportDirectory))
        {
            return;
        }

        // File names compare as Windows' file system does.
        var keptFileNames = new HashSet<string>(fileNames, StringComparer.OrdinalIgnoreCase);
        foreach (var path in Directory.GetFiles(ExportDirectory))
        {
            var fileName = Path.GetFileName(path);
            if (!keptFileNames.Contains(fileName))
            {
                Delete(fileName);
            }
        }
    }

    private static string FilePath(string fileName) => Path.Combine(ExportDirectory, fileName);
}
