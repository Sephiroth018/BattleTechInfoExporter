using System;
using System.Collections.Generic;
using System.IO;

namespace BattleTechInfoExporter;

/// <summary>The files in the folders of the mod's own folder, e.g. <c>exports</c> and <c>schemas</c>.</summary>
internal static class ModFolderFiles
{
    /// <summary>
    ///     Deletes every file in the folder but those named, e.g. the files of an earlier mod version, so a tool never
    ///     reads one that's no longer kept current.
    /// </summary>
    /// <exception cref="IOException">A file can't be deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">A file can't be deleted.</exception>
    internal static void DeleteAllExcept(string folder, IEnumerable<string> keptFileNames)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        // File names compare as Windows' file system does.
        var kept = new HashSet<string>(keptFileNames, StringComparer.OrdinalIgnoreCase);
        foreach (var path in Directory.GetFiles(folder))
        {
            var fileName = Path.GetFileName(path);
            if (!kept.Contains(fileName))
            {
                Delete(folder, fileName);
            }
        }
    }

    internal static void Delete(string folder, string fileName)
    {
        var path = Path.Combine(folder, fileName);
        if (!File.Exists(path))
        {
            return;
        }

        File.Delete(path);
        ModLog.Logger.Log($"Deleted {fileName} from {folder}");
    }
}
