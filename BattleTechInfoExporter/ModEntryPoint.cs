using System.Reflection;
using HarmonyLib;

namespace BattleTechInfoExporter;

/// <summary>ModTek calls every public static <c>Init</c> when it loads the mod.</summary>
public static class ModEntryPoint
{
    private static readonly AssemblyName ModAssemblyName = typeof(ModEntryPoint).Assembly.GetName();

    public static void Init()
    {
        Harmony.CreateAndPatchAll(typeof(ModEntryPoint).Assembly, ModAssemblyName.Name);
        ModLog.Logger.Log($"Loaded version {ModAssemblyName.Version.ToString(3)}");
    }
}
