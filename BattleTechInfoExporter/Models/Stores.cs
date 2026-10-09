using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The current system's stores; each is <c>null</c> if the system has none or the company can't use it. Their
///     stock changes only on arrival in a system.
/// </summary>
/// <param name="System">The system store, usable unless the reputation with the system's owner is LOATHED.</param>
/// <param name="Faction">The faction store, usable only while allied with its owner.</param>
/// <param name="BlackMarket">The black market, usable once the company has gained access to black markets.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Stores(Store? System, Store? Faction, Store? BlackMarket);

/// <param name="Faction">
///     The faction whose reputation sets the prices, by the price change in <see cref="Rules.ReputationLevels" />:
///     for the system store the system's owner, for the faction store its owner (usually the system's owner too,
///     and the faction the company must be allied with to use it), for the black market the pirates.
/// </param>
/// <param name="Components">The components in stock.</param>
/// <param name="Mechs">The whole mechs in stock.</param>
/// <param name="MechParts">The mech parts in stock.</param>
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
/// <param name="Mech">The mech, described in the catalog's <see cref="Catalog.MechDefinitions" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechForSale(DefinitionReference Mech, int? Count, int Price) : IForSale;

/// <param name="Count">The parts in stock; <c>null</c> for unlimited.</param>
/// <param name="Price">The C-Bills for one part, after the system's discount and the reputation's price change.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechPartsForSale(DefinitionReference Mech, int? Count, int Price)
    : MechPartsEntry(Mech), IForSale;
