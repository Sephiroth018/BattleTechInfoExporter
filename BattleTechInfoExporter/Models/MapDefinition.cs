using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A combat map and the terrain it is made of. A star system's contracts are fought on the maps whose biome the
///     system supports and whose tags its required and excluded map tags match, drawn by weight.
/// </summary>
/// <param name="Name">The map's friendly name.</param>
/// <param name="Biome">The map's biome, as the star systems refer to it.</param>
/// <param name="Tags">The map tags a star system's required and excluded map tags match against.</param>
/// <param name="Weight">The contract generator's draw weight among the maps a star system can use.</param>
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
