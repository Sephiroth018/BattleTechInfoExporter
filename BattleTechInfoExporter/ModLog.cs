using HBS.Logging;

namespace BattleTechInfoExporter;

internal static class ModLog
{
    /// <summary>Writes to ModTek's log under the mod's assembly name.</summary>
    internal static ILog Logger { get; } = HBS.Logging.Logger.GetLogger(typeof(ModLog).Assembly.GetName().Name);
}
