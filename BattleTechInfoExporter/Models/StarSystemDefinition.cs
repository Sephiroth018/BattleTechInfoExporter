using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A star system definition. The story swaps some star systems' definitions (e.g. to change their owner), so a
///     star system can have several; <c>star-systems.json</c> keys each star system by its active one.
/// </summary>
/// <param name="Name">The name, as in the references to the star system.</param>
/// <param name="Owner">The faction that owns the star system in this definition.</param>
/// <param name="Tags">The star system's tags, as its starmap panel shows them.</param>
/// <param name="Biomes">The biomes the star system's missions can be on.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StarSystemDefinition(
    string Name,
    DefinitionReference Owner,
    IReadOnlyList<DefinitionReference> Tags,
    IReadOnlyList<DefinitionReference> Biomes);
