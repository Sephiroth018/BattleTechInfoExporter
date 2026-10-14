using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A faction that offers contracts in a star system, with the factions they can be against.</summary>
/// <param name="Employer">A faction that can hire the company in the star system.</param>
/// <param name="Targets">
///     The factions it can hire the company against there, sorted by name: its enemies among the star system's
///     targets, except the company's allies.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ContractEmployer(DefinitionReference Employer, IReadOnlyList<DefinitionReference> Targets);
