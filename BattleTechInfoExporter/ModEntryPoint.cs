using System;
using System.IO;
using System.Linq;
using BattleTechInfoExporter.Export;
using HarmonyLib;

namespace BattleTechInfoExporter;

/// <summary>ModTek calls every public static <c>Init</c> when it loads the mod.</summary>
public static class ModEntryPoint
{
    /// <summary>Deletes the files in the exports folder the mod doesn't write, and applies the mod's Harmony patches.</summary>
    public static void Init()
    {
        try
        {
            ExportFileWriter.DeleteAllExcept(ExportFiles.All.Select(file => file.FileName));
        }
        // The exports still run; a leftover file only stays.
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ModLog.Logger.LogException(exception);
        }

        Harmony.CreateAndPatchAll(typeof(ModEntryPoint).Assembly, ModAssembly.Name);
    }
}
