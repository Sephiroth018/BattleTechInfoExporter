using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A faction's relations to the other factions, which decide whom it hires the company against.</summary>
/// <param name="Name">As in the references to the faction.</param>
/// <param name="Allies">
///     The factions allied with it, sorted by name; on its contracts, one of them not allied with the other side may
///     fight alongside it.
/// </param>
/// <param name="Enemies">
///     The factions hostile to it, sorted by name: those it can hire the company against where a star system lists
///     them as targets; while the company is allied with it, the company's reputation entries mark them as enemies.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record FactionDefinition(
    string Id,
    string Name,
    IReadOnlyList<DefinitionReference> Allies,
    IReadOnlyList<DefinitionReference> Enemies) : ICatalogEntry;
