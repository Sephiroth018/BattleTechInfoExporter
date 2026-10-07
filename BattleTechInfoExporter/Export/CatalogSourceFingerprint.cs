using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BattleTech;
using BattleTech.Assetbundles;
using BattleTech.Data;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Hashes the game's manifest entries for every resource type the catalog reads, so the catalog is rebuilt only
///     when its sources changed: a game update, a DLC bought or removed, a mod installed, changed, updated or
///     disabled, or a ModTek merge re-run, which rewrites the merged file. A change that touches no file the catalog
///     reads, e.g. a DLL mod patching definitions in memory, isn't detected.
/// </summary>
internal static class CatalogSourceFingerprint
{
    /// <summary>
    ///     The types the catalog reads that the career doesn't load (SimGameState.RequestDataManagerResources), so
    ///     <see cref="CatalogExporter" /> loads them.
    /// </summary>
    internal static readonly IReadOnlyList<BattleTechResourceType> ResourceTypesTheCareerDoesNotLoad =
    [
        BattleTechResourceType.VehicleDef,
        BattleTechResourceType.TurretDef,
        BattleTechResourceType.DesignMaskDef
    ];

    // Every type the catalog reads: the career's chassis, mechs and components, plus the ones above.
    private static readonly IReadOnlyList<BattleTechResourceType> ResourceTypes =
    [
        BattleTechResourceType.ChassisDef,
        BattleTechResourceType.MechDef,
        BattleTechResourceType.WeaponDef,
        BattleTechResourceType.AmmunitionBoxDef,
        BattleTechResourceType.HeatSinkDef,
        BattleTechResourceType.JumpJetDef,
        BattleTechResourceType.UpgradeDef,
        .. ResourceTypesTheCareerDoesNotLoad
    ];

    internal static string Compute(DataManager dataManager)
    {
        // Filtered by DLC ownership and without templates, like LoadRequest.AddAllOfTypeBlindLoadRequest loads
        // them; sorted, because the locator keeps no meaningful order.
        var lastWritesByPath = new Dictionary<string, string>(StringComparer.Ordinal);
        var entries = ResourceTypes
            .SelectMany(type => dataManager.ResourceLocator.AllEntriesOfResource(type, true))
            .Where(entry => !entry.IsTemplate)
            .Select(entry => Describe(entry, lastWritesByPath))
            .OrderBy(entry => entry, StringComparer.Ordinal);
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", entries)));
        return string.Concat(hash.Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
    }

    // The entry's file, or the asset bundle holding it (AssetBundleManager.AssetBundleNameToFilepath, which also
    // follows a mod's bundle), with its last write time, looked up once per file: a bundle holds many entries.
    private static string Describe(VersionManifestEntry entry, Dictionary<string, string> lastWritesByPath)
    {
        var path = entry.IsAssetBundled
            ? AssetBundleManager.AssetBundleNameToFilepath(entry.AssetBundleName)
            : entry.FilePath;
        if (!lastWritesByPath.TryGetValue(path, out var lastWrite))
        {
            // A missing file, e.g. a memory-only entry's, gets a fixed time instead of an exception.
            lastWrite = File.GetLastWriteTimeUtc(path).ToString("O", CultureInfo.InvariantCulture);
            lastWritesByPath[path] = lastWrite;
        }

        return $"{entry.Type}|{entry.Id}|{path}|{lastWrite}";
    }
}
