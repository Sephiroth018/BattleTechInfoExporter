using System.Reflection;
using HBS.Logging;

namespace BattleTechInfoExporter;

/// <summary>ModTek calls every public static <c>Init</c> when it loads the mod.</summary>
public static class ModEntryPoint
{
    private static readonly AssemblyName ModAssemblyName = typeof(ModEntryPoint).Assembly.GetName();
    private static readonly ILog Logger = HBS.Logging.Logger.GetLogger(ModAssemblyName.Name);

    public static void Init()
    {
        Logger.Log($"Loaded version {ModAssemblyName.Version.ToString(3)}");
    }
}
