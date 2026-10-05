using System.Collections.Generic;
using BattleTech;
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

/// <param name="Count">The copies in stock; <c>null</c>: unlimited.</param>
/// <param name="Price">The C-Bills for one copy, after the system's discount and the reputation's price change.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ComponentForSale(ComponentReference Component, int? Count, int Price);

/// <summary>
///     A whole mech, rarely in stock, described by its chassis as <see cref="StoredChassis" /> is. Buying it puts
///     the mech with its stock loadout into the mech bay.
/// </summary>
/// <param name="Count">The mechs in stock; <c>null</c>: unlimited.</param>
/// <param name="Price">The C-Bills for one mech, after the system's discount and the reputation's price change.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechForSale(
    DefinitionReference Mech,
    WeightClass WeightClass,
    float Tonnage,
    IReadOnlyList<LocationMaxArmor> MaxArmor,
    Hardpoints Hardpoints,
    int MaxJumpJets,
    int? Count,
    int Price);

/// <summary>Parts of a mech; with <see cref="Rules.MechPartsPerMech" /> of them they become that mech.</summary>
/// <param name="Count">The parts in stock; <c>null</c>: unlimited.</param>
/// <param name="Price">The C-Bills for one part, after the system's discount and the reputation's price change.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechPartsForSale(DefinitionReference Mech, int? Count, int Price);
