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
/// <param name="Tags">
///     The star system's tags, sorted, which the events' requirements check, also those its starmap panel doesn't
///     show.
/// </param>
/// <param name="Biomes">The biomes the star system's missions can be on, sorted by name.</param>
/// <param name="Maps">
///     The maps the star system's contracts can be fought on, as the contract generator selects them from its biomes
///     and map tags; each map's terrain is in the catalog's <c>mapDefinitions</c>.
/// </param>
/// <param name="Stores">
///     The item collections each of the star system's stores draws its stock from on arrival, every entry of each
///     once, by their ids in <see cref="Catalog.ItemCollectionDefinitions" />, sorted. A store without any doesn't
///     exist in the star system.
/// </param>
/// <param name="ContractEmployers">
///     The factions that can hire the company for the star system's generated contracts, sorted by name, each with
///     the factions it can hire it against, without the employers the story sets aside; a contract's own
///     requirements can narrow them further. Story and flashpoint contracts name their own.
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
    IReadOnlyList<string> Tags,
    IReadOnlyList<DefinitionReference> Biomes,
    IReadOnlyList<DefinitionReference> Maps,
    ByStore<IReadOnlyList<string>> Stores,
    IReadOnlyList<ContractEmployer> ContractEmployers,
    int Difficulty,
    bool CanTravelTo,
    Route? Route) : Reference(Id, Name);
