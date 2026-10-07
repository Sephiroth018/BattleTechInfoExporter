using System.Collections.Generic;
using JetBrains.Annotations;

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
    string ModVersion,
    ExportTrigger Trigger,
    string SourceFingerprint,
    IReadOnlyDictionary<string, ChassisDefinition> ChassisDefinitions,
    IReadOnlyDictionary<string, MechDefinition> MechDefinitions,
    IReadOnlyDictionary<string, VehicleDefinition> VehicleDefinitions,
    IReadOnlyDictionary<string, TurretDefinition> TurretDefinitions,
    ComponentDefinitions ComponentDefinitions,
    IReadOnlyDictionary<string, TerrainDefinition> TerrainDefinitions,
    IReadOnlyDictionary<string, BiomeDefinition> BiomeDefinitions,
    IReadOnlyDictionary<string, MapDefinition> MapDefinitions,
    IReadOnlyDictionary<string, StarSystemDefinition> StarSystemDefinitions) : ExportFile(ModVersion, null, Trigger);
