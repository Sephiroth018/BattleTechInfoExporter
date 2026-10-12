using System;
using System.IO;
using System.Linq;
using BattleTechInfoExporter.Export;
using HarmonyLib;

namespace BattleTechInfoExporter;

/// <summary>ModTek calls every public static <c>Init</c> when it loads the mod.</summary>
public static class ModEntryPoint
{
    /// <summary>
    ///     Deletes the files in the exports folder the mod doesn't write and the schemas it doesn't ship, and applies
    ///     the mod's Harmony patches.
    /// </summary>
    public static void Init()
    {
        try
        {
            ModFolderFiles.DeleteAllExcept(ExportFileWriter.ExportDirectory,
                ExportFiles.All.Select(file => file.FileName));
            // The build ships the schemas into the mod folder's schemas/.
            ModFolderFiles.DeleteAllExcept(Path.Combine(ModAssembly.Directory, "schemas"),
                ExportFiles.All.Select(file => ExportFiles.SchemaFileName(file.FileName)));
        }
        // The exports still run; a leftover file only stays.
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ModLog.Logger.LogException(exception);
        }

        Harmony.CreateAndPatchAll(typeof(ModEntryPoint).Assembly, ModAssembly.Name);
    }
}
