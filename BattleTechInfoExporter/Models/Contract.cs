using System.Collections.Generic;
using BattleTech.Framework;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     What a contract is in every state: offered (<see cref="Contract{TStarSystem}" />), accepted
///     (<see cref="ActiveContract" />) and fought (<see cref="MissionContract" />).
/// </summary>
/// <typeparam name="TStarSystem">How the contract's star system is exported: by reference or in full.</typeparam>
/// <param name="Id">The contract's data file; <c>null</c> where the game keeps no reference to it.</param>
/// <param name="Name">The contract's name, as the contract list shows it.</param>
/// <param name="Type">The mission type, described in <see cref="Rules.ContractTypes" />.</param>
/// <param name="DisplayStyle">Whether it's a regular, story, restoration or flashpoint contract.</param>
/// <param name="Employer">The faction that hires the company.</param>
/// <param name="Target">The faction the mission is fought against.</param>
/// <param name="Difficulty">
///     On the scale of <see cref="ReputationLevel.MaxContractDifficulty" />, not the 1-10 the skulls show
///     (two per skull).
/// </param>
/// <param name="StarSystem">The star system the mission is fought in.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record ContractIdentity<TStarSystem>(
    string? Id,
    string Name,
    DefinitionReference Type,
    ContractDisplayStyle DisplayStyle,
    DefinitionReference Employer,
    DefinitionReference Target,
    int Difficulty,
    TStarSystem StarSystem)
    where TStarSystem : Reference;

/// <summary>What the contract details show of a contract before its mission: offered or accepted.</summary>
/// <typeparam name="TStarSystem">How the contract's star system is exported: by reference or in full.</typeparam>
/// <param name="Description">The contract's description, as the contract details show it.</param>
/// <param name="LanceLimits">The limits on the lance the company can deploy on the mission.</param>
/// <param name="Biome"><c>null</c> where the game has no biome for the contract.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record ContractBriefing<TStarSystem>(
    string? Id,
    string Name,
    DefinitionReference Type,
    ContractDisplayStyle DisplayStyle,
    DefinitionReference Employer,
    DefinitionReference Target,
    int Difficulty,
    TStarSystem StarSystem,
    string Description,
    LanceLimits LanceLimits,
    DefinitionReference? Biome)
    : ContractIdentity<TStarSystem>(Id, Name, Type, DisplayStyle, Employer, Target, Difficulty, StarSystem)
    where TStarSystem : Reference;

/// <summary>A contract the Command Center offers, as its contract list and details show it.</summary>
/// <typeparam name="TStarSystem">
///     <see cref="DefinitionReference" /> for a contract in the current star system, <see cref="StarSystem" /> for a
///     travel contract.
/// </typeparam>
/// <param name="StarSystem">
///     The star system the mission is fought in: by reference in the current star system; in full for a travel
///     contract, as in <c>star-systems.json</c>, with the trip there.
/// </param>
/// <param name="MeetsReputation">
///     Whether the reputation with the employer allows taking the contract; the list greys it out otherwise. Story,
///     restoration and flashpoint contracts, and employers that don't gain reputation, are always allowed.
/// </param>
/// <param name="Negotiation">The terms the contract can be accepted with: negotiated or fixed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Contract<TStarSystem>(
    string? Id,
    string Name,
    DefinitionReference Type,
    ContractDisplayStyle DisplayStyle,
    DefinitionReference Employer,
    DefinitionReference Target,
    int Difficulty,
    TStarSystem StarSystem,
    string Description,
    LanceLimits LanceLimits,
    DefinitionReference? Biome,
    bool MeetsReputation,
    Negotiation Negotiation)
    : ContractBriefing<TStarSystem>(
        Id,
        Name,
        Type,
        DisplayStyle,
        Employer,
        Target,
        Difficulty,
        StarSystem,
        Description,
        LanceLimits,
        Biome)
    where TStarSystem : Reference;

/// <summary>The accepted travel contract, until the company proceeds with it on arrival or breaks it.</summary>
/// <param name="Terms">The terms the contract was accepted with.</param>
/// <param name="ArrivesOnDay">
///     While travelling to the star system, the day of arrival, as in <see cref="Travel.ArrivesOnDay" />;
///     <c>null</c> otherwise.
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
    DefinitionReference StarSystem,
    string Description,
    LanceLimits LanceLimits,
    DefinitionReference? Biome,
    NegotiationOption Terms,
    int? ArrivesOnDay)
    : ContractBriefing<DefinitionReference>(
        Id,
        Name,
        Type,
        DisplayStyle,
        Employer,
        Target,
        Difficulty,
        StarSystem,
        Description,
        LanceLimits,
        Biome);

/// <summary>The contract's terms.</summary>
/// <param name="CanNegotiate">
///     Whether the terms are negotiated with the sliders (<see cref="ValuesByShare" />) or fixed
///     (<see cref="FixedTerms" />).
/// </param>
/// <param name="ValuesByShare">
///     The values at each slider position; <c>null</c> when the contract can't be negotiated. A row is not a deal:
///     the pay comes from the row at the pay share, the salvage from the row at the salvage share and the reputation
///     from the row at the share left over.
/// </param>
/// <param name="FixedTerms">The terms of a contract that can't be negotiated; <c>null</c> otherwise.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Negotiation(
    bool CanNegotiate,
    IReadOnlyList<ValuesAtShare>? ValuesByShare,
    NegotiationOption? FixedTerms);

/// <summary>What one slider position gives, each value for its own slider.</summary>
/// <param name="Share">The slider's position in percent.</param>
/// <param name="Pay">The pay when the pay slider is at this share.</param>
/// <param name="Salvage">The salvage when the salvage slider is at this share.</param>
/// <param name="Reputation">The reputation changes when this share is left over for reputation.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ValuesAtShare(int Share, int Pay, Salvage Salvage, ReputationChange Reputation);

/// <param name="PayShare">The pay slider's position in percent; <c>null</c> for fixed terms.</param>
/// <param name="SalvageShare">The salvage slider's position in percent; <c>null</c> for fixed terms.</param>
/// <param name="Pay">The pay in C-Bills.</param>
/// <param name="Salvage">The salvage the company gets.</param>
/// <param name="Reputation">The reputation changes when the contract succeeds.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record NegotiationOption(
    int? PayShare,
    int? SalvageShare,
    int Pay,
    Salvage Salvage,
    ReputationChange Reputation);

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
internal sealed record ReputationChange(int? Employer, int? Target);

/// <param name="Total">The salvage the company gets, including the priority salvage.</param>
/// <param name="Priority">The part of the salvage the company picks itself; the rest is assigned to it.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Salvage(int Total, int Priority);

/// <summary>The lance the contract allows; every limit is <c>null</c> when there is none.</summary>
/// <param name="MaxMechs">The most mechs the lance can deploy.</param>
/// <param name="MinTonnage">The least total tonnage of the lance's mechs.</param>
/// <param name="MaxTonnage">The most total tonnage of the lance's mechs.</param>
/// <param name="Mechs">The tonnage limits of each mech slot, one per mech allowed; <c>null</c> when no slot has any.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LanceLimits(
    int MaxMechs,
    float? MinTonnage,
    float? MaxTonnage,
    IReadOnlyList<MechSlotLimits>? Mechs);

/// <param name="MinTonnage">The least tonnage of the mech in the slot; <c>null</c> without a limit.</param>
/// <param name="MaxTonnage">The most tonnage of the mech in the slot; <c>null</c> without a limit.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechSlotLimits(float? MinTonnage, float? MaxTonnage);
