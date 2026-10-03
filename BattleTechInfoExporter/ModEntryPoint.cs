using System.Reflection;
using HarmonyLib;
using HBS.Logging;

namespace BattleTechInfoExporter;

/// <summary>ModTek calls every public static <c>Init</c> when it loads the mod.</summary>
public static class ModEntryPoint
{
    private static readonly AssemblyName ModAssemblyName = typeof(ModEntryPoint).Assembly.GetName();

    internal static ILog Logger { get; } = HBS.Logging.Logger.GetLogger(ModAssemblyName.Name);

    public static void Init()
    {
        Harmony.CreateAndPatchAll(typeof(ModEntryPoint).Assembly, ModAssemblyName.Name);
        Logger.Log($"Loaded version {ModAssemblyName.Version.ToString(3)}");
    }
}
