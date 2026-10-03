using HBS.Logging;

namespace BattleTechInfoExporter;

/// <summary>ModTek calls every public static <c>Init</c> when it loads the mod.</summary>
public static class ModEntryPoint
{
    private static readonly ILog Logger = HBS.Logging.Logger.GetLogger("BattleTechInfoExporter");

    public static void Init()
    {
        var version = typeof(ModEntryPoint).Assembly.GetName().Version;
        Logger.Log($"Loaded version {version.ToString(3)}");
    }
}
