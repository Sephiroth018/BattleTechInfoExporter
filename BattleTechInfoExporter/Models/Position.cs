using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Travel">Where the ship is headed; <c>null</c> while it's in a system.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Position(
    DefinitionReference System,
    DefinitionReference Owner,
    SimGameTravelStatus TravelStatus,
    Travel? Travel);
