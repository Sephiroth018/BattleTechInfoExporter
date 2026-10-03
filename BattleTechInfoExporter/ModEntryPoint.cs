using HarmonyLib;

namespace BattleTechInfoExporter;

/// <summary>ModTek calls every public static <c>Init</c> when it loads the mod.</summary>
public static class ModEntryPoint
{
    public static void Init()
    {
        Harmony.CreateAndPatchAll(typeof(ModEntryPoint).Assembly, ModAssembly.Name);
    }
}
