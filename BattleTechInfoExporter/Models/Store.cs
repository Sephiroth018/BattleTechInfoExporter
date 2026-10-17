using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The current star system's stores the company can use.</summary>
/// <param name="System">
///     The system store; <c>null</c> when the star system has none or the reputation with its owner is
///     <c>LOATHED</c>.
/// </param>
/// <param name="Faction">
///     The faction store; <c>null</c> when the star system has none or the company isn't allied with its owner.
/// </param>
/// <param name="BlackMarket">
///     The black market; <c>null</c> when the star system has none or the company hasn't gained access to black
///     markets.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StarSystemStores(Store? System, Store? Faction, Store? BlackMarket);

/// <summary>A store's stock and prices.</summary>
/// <param name="Faction">
///     The faction whose reputation sets the prices, by the price change in <see cref="Rules.ReputationLevels" />:
///     the star system's owner for the system store, the store's own faction for the faction store, the pirates for
///     the black market.
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

/// <summary>A component in stock.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ComponentForSale(ComponentReference Component, int? Count, int Price)
    : ComponentEntry(Component), IForSale;

/// <summary>A whole mech in stock. Buying it puts the mech with its stock loadout into the mech bay.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechForSale(DefinitionReference Mech, int? Count, int Price) : MechEntry(Mech), IForSale;

/// <summary>Mech parts in stock.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechPartsForSale(DefinitionReference Mech, int? Count, int Price)
    : MechPartsEntry(Mech), IForSale;
