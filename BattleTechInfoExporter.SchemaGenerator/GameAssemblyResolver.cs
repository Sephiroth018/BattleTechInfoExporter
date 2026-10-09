using System.IO;
using System.Runtime.Loader;

namespace BattleTechInfoExporter.SchemaGenerator;

/// <summary>
///     Loads the game's assemblies the mod's models refer to (game enums, the JetBrains annotations) from the
///     install, as the game does; they're never copied into a build output.
/// </summary>
internal static class GameAssemblyResolver
{
    internal static void Register(string gameDirectory)
    {
        var managedDirectory = Path.Combine(gameDirectory, "BattleTech_Data", "Managed");
        // Only asked for what .NET doesn't provide itself, so the game's own mscorlib is never loaded.
        AssemblyLoadContext.Default.Resolving += (context, name) =>
            Path.Combine(managedDirectory, name.Name + ".dll") is var path && File.Exists(path)
                ? context.LoadFromAssemblyPath(path)
                : null;
    }
}
