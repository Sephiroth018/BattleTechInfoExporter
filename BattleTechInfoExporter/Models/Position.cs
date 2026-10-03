using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Position(
    DefinitionReference System,
    DefinitionReference Owner,
    SimGameTravelStatus TravelStatus);
