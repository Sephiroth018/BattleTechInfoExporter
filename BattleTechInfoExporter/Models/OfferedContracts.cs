using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The contracts the Command Center offers, without the active contract.</summary>
/// <param name="InCurrentStarSystem">The contracts fought in the current star system.</param>
/// <param name="Travel">The travel contracts, fought in another star system the company has to travel to first.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record OfferedContracts(
    IReadOnlyList<Contract<DefinitionReference>> InCurrentStarSystem,
    IReadOnlyList<Contract<StarSystem>> Travel);
