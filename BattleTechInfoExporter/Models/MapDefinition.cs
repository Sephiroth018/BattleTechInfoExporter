using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A combat map some star system's contracts can be fought on, and the terrain it is made of. The contract
///     generator draws a star system's map among the maps <c>star-systems.json</c> lists for the system, by weight.
/// </summary>
/// <param name="Name">The map's friendly name.</param>
/// <param name="Biome">The map's biome, as the star systems refer to it.</param>
/// <param name="Tags">The map tags the star systems' required and excluded map tags select the map by.</param>
/// <param name="Weight">The contract generator's draw weight among a star system's maps.</param>
/// <param name="TerrainCoverage">
///     Each terrain's share of the map's playable cells, keyed by terrain id, plus <c>none</c> for the cells with
///     no terrain (bare biome). Cells are all the same size, so a share is an area share; the shares sum to 1.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MapDefinition(
    string Name,
    DefinitionReference Biome,
    IReadOnlyList<string> Tags,
    int Weight,
    IReadOnlyDictionary<string, double> TerrainCoverage);
