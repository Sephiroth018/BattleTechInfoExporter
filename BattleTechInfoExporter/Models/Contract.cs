using System.Collections.Generic;
using BattleTech.Framework;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A contract the Command Center offers, as its contract list and details show it.</summary>
/// <param name="Id">The contract's data file; <c>null</c> where the game keeps no reference to it.</param>
/// <param name="Type">The mission type, described in <see cref="Rules.ContractTypes" />.</param>
/// <param name="DisplayStyle">Whether it's a regular, story, restoration or flashpoint contract.</param>
/// <param name="Difficulty">
///     On the scale of <see cref="ReputationLevel.MaxContractDifficulty" />, not the 1-10 the skulls show
///     (two per skull).
/// </param>
/// <param name="MeetsReputation">
///     Whether the reputation with the employer allows taking the contract; the list greys it out otherwise. Story,
///     restoration and flashpoint contracts, and employers that don't gain reputation, are always allowed.
/// </param>
/// <param name="Biome"><c>null</c> where the game has no biome for the contract.</param>
/// <param name="Travel"><c>null</c> for a contract in the current system.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Contract(
    string? Id,
    string Name,
    DefinitionReference Type,
    ContractDisplayStyle DisplayStyle,
    DefinitionReference Employer,
    DefinitionReference Target,
    int Difficulty,
    bool MeetsReputation,
    Negotiation Negotiation,
    LanceLimits Lance,
    DefinitionReference? Biome,
    ContractTravel? Travel);

/// <summary>The contract's terms.</summary>
/// <param name="Options">
///     Every combination of the pay and salvage sliders the negotiation allows: their shares add up to at most 100,
///     reputation gets the rest. When the employer doesn't gain reputation, they always add up to 100. A single
///     option with the fixed terms when the contract can't be negotiated.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Negotiation(bool CanNegotiate, IReadOnlyList<NegotiationOption> Options);

/// <param name="PayShare">The pay slider's position in percent; <c>null</c> for fixed terms.</param>
/// <param name="SalvageShare">The salvage slider's position in percent; <c>null</c> for fixed terms.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record NegotiationOption(
    int? PayShare,
    int? SalvageShare,
    int Pay,
    Salvage Salvage,
    ReputationChange Reputation);

/// <summary>The reputation changes of a successful contract, as the negotiation shows them.</summary>
/// <param name="Employer">The gain with the employer; <c>null</c> when the employer doesn't gain reputation.</param>
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
/// <param name="Mechs">The tonnage limits of each mech slot, one per mech allowed; <c>null</c> when no slot has any.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LanceLimits(
    int MaxMechs,
    float? MinTonnage,
    float? MaxTonnage,
    IReadOnlyList<MechSlotLimits>? Mechs);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechSlotLimits(float? MinTonnage, float? MaxTonnage);

/// <param name="Tags">The planet tags the starmap shows for the system, e.g. its industry.</param>
/// <param name="Days">
///     The travel days to the system, as the contract details show them; <c>null</c> if the game finds no route.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ContractTravel(DefinitionReference System, IReadOnlyList<DefinitionReference> Tags, int? Days);
