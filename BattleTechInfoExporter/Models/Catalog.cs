using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The root of <c>catalog.json</c>: every chassis, mech, vehicle, turret, component, terrain, biome, map, event,
///     item collection and faction definition the game has loaded, keyed by id. It describes the game, not the
///     career, so it only changes with the game's data. Left out are the tutorial's target dummies and target
///     vehicles, the copies of stock mechs the game makes for unlocked skins, the game's internal melee and AI weapons
///     and the ammunition boxes for weapons that carry their own ammunition, such as flamers.
/// </summary>
/// <param name="SourceFingerprint">
///     A hash of the game's data files the catalog was built from: of every one it reads, its id, file and last
///     write time; the catalog is rebuilt when it differs from the game's current data. A game update, a DLC bought
///     or removed, a mod installed, updated, removed or disabled, or a ModTek merge re-run changes it; a DLL mod
///     patching definitions in memory doesn't, so deleting the file is what forces a rebuild then.
/// </param>
/// <param name="ChassisDefinitions">The chassis, keyed by chassis id.</param>
/// <param name="MechDefinitions">The mechs, keyed by mech id.</param>
/// <param name="VehicleDefinitions">The vehicles, keyed by vehicle id.</param>
/// <param name="TurretDefinitions">The turrets, keyed by turret id.</param>
/// <param name="ComponentDefinitions">The components, grouped by type and keyed by component id.</param>
/// <param name="TerrainDefinitions">The terrains, keyed by design mask id.</param>
/// <param name="BiomeDefinitions">The biomes, keyed by the biome id the star systems and maps refer to.</param>
/// <param name="MapDefinitions">The maps a star system's contracts can be fought on, keyed by map id.</param>
/// <param name="EventDefinitions">The events, keyed by event id.</param>
/// <param name="ItemCollectionDefinitions">
///     The item collections the stores draw their stock from and the rewards are drawn from, keyed by collection
///     id; a star system's stores name theirs in <see cref="Starmap.StarSystems" />.
/// </param>
/// <param name="FactionDefinitions">The factions' allies and enemies, keyed by faction id.</param>
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
    IReadOnlyDictionary<string, EventDefinition> EventDefinitions,
    IReadOnlyDictionary<string, ItemCollectionDefinition> ItemCollectionDefinitions,
    IReadOnlyDictionary<string, FactionDefinition> FactionDefinitions) : ExportFile
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
