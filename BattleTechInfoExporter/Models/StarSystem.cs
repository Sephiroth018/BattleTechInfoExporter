using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Tags">The system's planet tags, as its starmap panel shows them.</param>
/// <param name="Biomes">The biomes the system's missions can be on.</param>
/// <param name="Difficulty">The system's difficulty, as the starmap shows it.</param>
/// <param name="CanTravelTo">Whether the system's travel requirements are met; story systems stay locked until then.</param>
/// <param name="TravelDays">
///     The days the trip from the current system takes, as the starmap shows them; 0 for the current system and
///     <c>null</c> where the game finds no route, always while <paramref name="CanTravelTo" /> is false.
/// </param>
/// <param name="TravelCost">The C-Bills the trip costs, under the same rules as <paramref name="TravelDays" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StarSystem(
    DefinitionReference System,
    DefinitionReference Owner,
    IReadOnlyList<DefinitionReference> Tags,
    IReadOnlyList<DefinitionReference> Biomes,
    int Difficulty,
    bool CanTravelTo,
    int? TravelDays,
    int? TravelCost);
