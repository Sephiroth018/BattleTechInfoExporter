using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The current system's stores; each is <c>null</c> if the system has none or the company can't use it. Their
///     stock changes only on arrival in a system.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Stores(Store? System, Store? Faction, Store? BlackMarket);

/// <param name="Faction">
///     The faction whose reputation sets the prices, by the price change in <see cref="Rules.ReputationLevels" />:
///     for the system store the system's owner, for the faction store its owner (usually the system's owner too,
///     and the faction the company must be allied with to use it), for the black market the pirates.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Store(
    DefinitionReference Faction,
    IReadOnlyList<ComponentForSale> Components,
    IReadOnlyList<MechForSale> Mechs,
    IReadOnlyList<MechPartsForSale> MechParts);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ComponentForSale(ComponentReference Component, int? Count, int Price)
    : ComponentEntry(Component), IForSale;

/// <summary>
///     A whole mech, rarely in stock, described in the catalog. Buying it puts the mech with its stock loadout into
///     the mech bay.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechForSale(DefinitionReference Mech, int? Count, int Price) : IForSale;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechPartsForSale(DefinitionReference Mech, int? Count, int Price)
    : MechPartsEntry(Mech), IForSale;
