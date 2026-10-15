using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A faction's relations to the other factions, which decide whom it hires the company against. Some faction
///     references have no entry, such as <c>Player1sMercUnit</c> and <c>INVALID_UNSET</c>.
/// </summary>
/// <param name="Name">As in the references to the faction; not unique, since several ids can share a name.</param>
/// <param name="Allies">
///     The factions allied with it, sorted by name; on its contracts, one of them not allied with the other side may
///     fight alongside it. The faction's own list: an ally needn't list it back.
/// </param>
/// <param name="Enemies">
///     The factions hostile to it, sorted by name: those it can hire the company against where a star system lists
///     them as targets; while the company is allied with it, the company's reputation entries mark them as enemies.
///     The faction's own list: an enemy needn't list it back.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record FactionDefinition(
    string Id,
    string Name,
    IReadOnlyList<DefinitionReference> Allies,
    IReadOnlyList<DefinitionReference> Enemies) : ICatalogEntry;
