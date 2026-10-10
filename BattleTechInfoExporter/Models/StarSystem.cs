using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A star system with its active definition and its career state. The story swaps some star systems' definitions
///     (e.g. to change their owner); the id and what the definition holds are the active one's.
/// </summary>
/// <param name="Id">The active definition's id, as in the references to the star system.</param>
/// <param name="Name">The name, as in the references to the star system.</param>
/// <param name="Owner">The faction that owns the star system.</param>
/// <param name="Tags">The star system's tags, as its starmap panel shows them.</param>
/// <param name="Biomes">The biomes the star system's missions can be on.</param>
/// <param name="Maps">
///     The maps the star system's contracts can be fought on, as the contract generator selects them from its biomes
///     and map tags; each map's terrain is in the catalog's <c>mapDefinitions</c>.
/// </param>
/// <param name="Difficulty">The star system's difficulty, as the starmap shows it.</param>
/// <param name="CanTravelTo">
///     Whether the star system's travel requirements are met; story systems stay locked until then.
/// </param>
/// <param name="Route">
///     The trip from the current star system; <c>null</c> where the game finds no route, always while
///     <paramref name="CanTravelTo" /> is false.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StarSystem(
    string Id,
    string Name,
    DefinitionReference Owner,
    IReadOnlyList<DefinitionReference> Tags,
    IReadOnlyList<DefinitionReference> Biomes,
    IReadOnlyList<DefinitionReference> Maps,
    int Difficulty,
    bool CanTravelTo,
    Route? Route) : Reference(Id, Name);
