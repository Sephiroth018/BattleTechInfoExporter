using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The root of <c>catalog.json</c>: every chassis, mech, vehicle, turret, component, terrain, biome, map and star
///     system definition the game has loaded, keyed by id. It describes the game, not the career, so it only changes
///     with the game's data.
/// </summary>
/// <param name="SourceFingerprint">
///     A hash of the game's data the catalog was built from (<see cref="Export.CatalogSourceFingerprint" />); the
///     catalog is rebuilt when it differs from the game's current data.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Catalog(
    string SourceFingerprint,
    IReadOnlyDictionary<string, ChassisDefinition> ChassisDefinitions,
    IReadOnlyDictionary<string, MechDefinition> MechDefinitions,
    IReadOnlyDictionary<string, VehicleDefinition> VehicleDefinitions,
    IReadOnlyDictionary<string, TurretDefinition> TurretDefinitions,
    ComponentDefinitions ComponentDefinitions,
    IReadOnlyDictionary<string, TerrainDefinition> TerrainDefinitions,
    IReadOnlyDictionary<string, BiomeDefinition> BiomeDefinitions,
    IReadOnlyDictionary<string, MapDefinition> MapDefinitions,
    IReadOnlyDictionary<string, StarSystemDefinition> StarSystemDefinitions) : ExportFile
{
    internal const string FileName = "catalog.json";

    /// <summary>
    ///     Whether the catalog in the file is current: written by this mod version from the same sources. An unreadable
    ///     or malformed file isn't; the rebuild replaces it.
    /// </summary>
    internal static bool IsCurrent(string sourceFingerprint)
    {
        try
        {
            var header = ReadHeader(FileName, nameof(ModVersion), nameof(SourceFingerprint));
            return header is not null
                   && header[nameof(ModVersion)] == ModAssembly.Version
                   && header[nameof(SourceFingerprint)] == sourceFingerprint;
        }
        catch (Exception exception) when (exception is JsonException or IOException)
        {
            ModLog.Logger.LogWarning($"Rebuilding the unreadable {FileName}: {exception.Message}");
            return false;
        }
    }
}
