using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Travel">Where the ship is headed; <c>null</c> while it's in a star system.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Position(DefinitionReference StarSystem, SimGameTravelStatus TravelStatus, Travel? Travel);
