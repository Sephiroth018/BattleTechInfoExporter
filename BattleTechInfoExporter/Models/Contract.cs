using System.Collections.Generic;
using BattleTech.Framework;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     What a contract is in every state: offered (<see cref="OfferedContract" />), accepted
///     (<see cref="ActiveContract" />) and fought (<see cref="MissionContract" />). Each state declares the star
///     system the mission is fought in, by reference or in full.
/// </summary>
/// <param name="Id">The contract's data file; <c>null</c> when the game keeps no reference to it.</param>
/// <param name="Name">The contract's name, as the contract list shows it.</param>
/// <param name="Type">The mission type, described in <see cref="Rules.ContractTypes" />.</param>
/// <param name="DisplayStyle">Whether it's a regular, story, restoration or flashpoint contract.</param>
/// <param name="Employer">The faction that hires the company.</param>
/// <param name="Target">
///     The faction the mission is fought against; <c>INVALID_UNSET</c>, which has no
///     <see cref="Catalog.FactionDefinitions" /> entry, for a story contract that only travels.
/// </param>
/// <param name="Difficulty">
///     On the scale of <see cref="ReputationLevel.MaxContractDifficulty" />, not the 1-10 the skulls show
///     (two per skull).
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record ContractIdentity(
    string? Id,
    string Name,
    DefinitionReference Type,
    ContractDisplayStyle DisplayStyle,
    DefinitionReference Employer,
    DefinitionReference Target,
    int Difficulty);

/// <summary>What the contract details show of a contract before its mission: offered or accepted.</summary>
/// <param name="Description">The contract's description, as the contract details show it.</param>
/// <param name="LanceLimits">The limits on the lance the company can deploy on the mission.</param>
/// <param name="Biome">The contract's biome; <c>null</c> when the game has none for it.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record ContractBriefing(
    string? Id,
    string Name,
    DefinitionReference Type,
    ContractDisplayStyle DisplayStyle,
    DefinitionReference Employer,
    DefinitionReference Target,
    int Difficulty,
    string Description,
    LanceLimits LanceLimits,
    DefinitionReference? Biome)
    : ContractIdentity(Id, Name, Type, DisplayStyle, Employer, Target, Difficulty);

/// <summary>A contract the Command Center offers, as its contract list and details show it.</summary>
/// <param name="MeetsReputation">
///     Whether the reputation with the employer allows taking the contract; the list greys it out otherwise. Story,
///     restoration and flashpoint contracts, and employers that don't gain reputation, are always allowed.
/// </param>
/// <param name="Negotiation">The terms the contract can be accepted with: negotiated or fixed.</param>
/// <param name="StarSystem">
///     The star system the mission is fought in, in full, as in <see cref="Starmap.StarSystems" />, with the trip
///     there: the current one, whose route takes 0 days and costs 0 C-Bills, or another the company travels to first.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record OfferedContract(
    string? Id,
    string Name,
    DefinitionReference Type,
    ContractDisplayStyle DisplayStyle,
    DefinitionReference Employer,
    DefinitionReference Target,
    int Difficulty,
    string Description,
    LanceLimits LanceLimits,
    DefinitionReference? Biome,
    bool MeetsReputation,
    Negotiation Negotiation,
    StarSystem StarSystem)
    : ContractBriefing(Id, Name, Type, DisplayStyle, Employer, Target, Difficulty, Description, LanceLimits, Biome);

/// <summary>The accepted travel contract, until the company proceeds with it on arrival or breaks it.</summary>
/// <param name="Terms">The terms the contract was accepted with.</param>
/// <param name="ArrivesOnDay">
///     The day of arrival in the star system, as in <see cref="Travel.ArrivesOnDay" />; <c>null</c> when the ship
///     isn't travelling there.
/// </param>
/// <param name="StarSystem">
///     The star system the mission is fought in, in full, as in <see cref="Starmap.StarSystems" />, with the trip
///     there.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ActiveContract(
    string? Id,
    string Name,
    DefinitionReference Type,
    ContractDisplayStyle DisplayStyle,
    DefinitionReference Employer,
    DefinitionReference Target,
    int Difficulty,
    string Description,
    LanceLimits LanceLimits,
    DefinitionReference? Biome,
    AcceptedTerms Terms,
    int? ArrivesOnDay,
    StarSystem StarSystem)
    : ContractBriefing(Id, Name, Type, DisplayStyle, Employer, Target, Difficulty, Description, LanceLimits, Biome);

/// <summary>
///     The terms a contract can be accepted with; each value of <see cref="CanNegotiate" /> has a shape with fields
///     of its own.
/// </summary>
/// <param name="CanNegotiate">
///     Whether the terms are negotiated with the sliders (<see cref="NegotiableTerms" />) or fixed
///     (<see cref="FixedTerms" />).
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record Negotiation(bool CanNegotiate);

/// <summary>The terms of a contract negotiated with the sliders.</summary>
/// <param name="ValuesByShare">
///     The values at each slider position. A row is not a deal: the pay comes from the row at the pay share, the
///     salvage from the row at the salvage share and the reputation from the row at the share left over. The pay
///     and salvage shares add up to at most 100, and to exactly 100 when the employer gains no reputation.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record NegotiableTerms(IReadOnlyList<ValuesAtShare> ValuesByShare) : Negotiation(true);

/// <summary>The terms of a contract that can't be negotiated.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record FixedTerms(int Pay, Salvage Salvage, ReputationChange Reputation)
    : Negotiation(false), IContractTerms;

/// <summary>What one slider position gives, each value for its own slider.</summary>
/// <param name="SharePercent">The slider's position, from 0 to 100.</param>
/// <param name="Pay">The pay when the pay slider is at this share.</param>
/// <param name="Salvage">The salvage when the salvage slider is at this share.</param>
/// <param name="Reputation">The reputation changes when this share is left over for reputation.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ValuesAtShare(int SharePercent, int Pay, Salvage Salvage, ReputationChange Reputation);

/// <summary>The terms a contract was accepted with.</summary>
/// <param name="Shares">The slider positions the terms were negotiated at; <c>null</c> when the terms are fixed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record AcceptedTerms(
    NegotiatedShares? Shares,
    int Pay,
    Salvage Salvage,
    ReputationChange Reputation) : IContractTerms;

/// <summary>The slider positions a contract's terms were negotiated at.</summary>
/// <param name="PayPercent">The pay slider's position, from 0 to 100.</param>
/// <param name="SalvagePercent">The salvage slider's position, from 0 to 100.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record NegotiatedShares(int PayPercent, int SalvagePercent);

/// <summary>
///     The reputation changes of a contract: of its success, as the negotiation shows them, or of a mission's outcome.
/// </summary>
/// <param name="Employer">
///     The change, usually a gain, with the employer; <c>null</c> when the employer doesn't gain reputation.
/// </param>
/// <param name="Target">
///     The change, usually a loss, with the target; <c>null</c> when the target doesn't gain reputation.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal record ReputationChange(int? Employer, int? Target);

/// <summary>The salvage of a contract, in items.</summary>
/// <param name="Total">The salvage the company gets, including the priority salvage.</param>
/// <param name="Priority">The part of the salvage the company picks itself; the rest is assigned to it.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Salvage(int Total, int Priority);

/// <summary>The lance the contract allows.</summary>
/// <param name="MaxMechs">The most mechs the lance can deploy.</param>
/// <param name="MinTonnage">The least total tonnage of the lance's mechs; <c>null</c> when there's no limit.</param>
/// <param name="MaxTonnage">The most total tonnage of the lance's mechs; <c>null</c> when there's no limit.</param>
/// <param name="Mechs">The tonnage limits of each mech slot, one per mech allowed; <c>null</c> when no slot has any.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LanceLimits(
    int MaxMechs,
    float? MinTonnage,
    float? MaxTonnage,
    IReadOnlyList<MechSlotLimits>? Mechs);

/// <summary>The tonnage limits of one mech slot of the lance.</summary>
/// <param name="MinTonnage">The least tonnage of the mech in the slot; <c>null</c> when there's no limit.</param>
/// <param name="MaxTonnage">The most tonnage of the mech in the slot; <c>null</c> when there's no limit.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechSlotLimits(float? MinTonnage, float? MaxTonnage);
