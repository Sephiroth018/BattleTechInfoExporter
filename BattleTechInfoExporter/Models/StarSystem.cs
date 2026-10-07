using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Name">The name, as in the references to the star system.</param>
/// <param name="Tags">The star system's tags, as its starmap panel shows them.</param>
/// <param name="Biomes">The biomes the star system's missions can be on.</param>
/// <param name="Difficulty">The star system's difficulty, as the starmap shows it.</param>
/// <param name="CanTravelTo">
///     Whether the star system's travel requirements are met; story systems stay locked until then.
/// </param>
/// <param name="TravelDays">
///     The days the trip from the current star system takes, as the starmap shows them; 0 for the current star
///     system and <c>null</c> where the game finds no route, always while <paramref name="CanTravelTo" /> is false.
/// </param>
/// <param name="TravelCost">The C-Bills the trip costs, under the same rules as <paramref name="TravelDays" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StarSystem(
    string Name,
    DefinitionReference Owner,
    IReadOnlyList<DefinitionReference> Tags,
    IReadOnlyList<DefinitionReference> Biomes,
    int Difficulty,
    bool CanTravelTo,
    int? TravelDays,
    int? TravelCost);
