using System;
using System.IO;
using System.Reflection;

namespace BattleTechInfoExporter;

/// <summary>The mod's identity and location, taken from its assembly.</summary>
internal static class ModAssembly
{
    private static readonly Assembly Assembly = typeof(ModAssembly).Assembly;

    internal static string Name { get; } = Assembly.GetName().Name;

    internal static string Version { get; } = Assembly.GetName().Version.ToString(3);

    /// <summary>The mod's folder in the game's <c>Mods</c> folder, where ModTek loaded the DLL from.</summary>
    internal static string Directory { get; } = Path.GetDirectoryName(Assembly.Location)
                                                ?? throw new InvalidOperationException(
                                                    $"No directory for the mod's assembly at '{Assembly.Location}'");
}
