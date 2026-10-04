using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Tags">The system's planet tags, as its starmap panel shows them.</param>
/// <param name="Biomes">The biomes the system's missions can be on.</param>
/// <param name="Travel">Where the ship is headed; <c>null</c> while it's in a system.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Position(
    DefinitionReference System,
    DefinitionReference Owner,
    IReadOnlyList<DefinitionReference> Tags,
    IReadOnlyList<DefinitionReference> Biomes,
    SimGameTravelStatus TravelStatus,
    Travel? Travel);
