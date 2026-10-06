using System.Collections.Generic;
using BattleTech;
using BattleTech.Framework;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The root of <c>mission-outcome.json</c>: the latest mission, as it ended, before the salvage is chosen.</summary>
/// <param name="State">Whether the contract was completed, retreated from or failed.</param>
/// <param name="IsGoodFaithEffort">
///     Whether a retreated or failed contract still counts as a good faith effort, which softens its penalties.
/// </param>
/// <param name="Payment">The C-Bills paid, with the good faith modifier and the bonuses applied.</param>
/// <param name="Reputation">The final reputation changes with the employer and the target.</param>
/// <param name="MercenaryReviewBoardReputation">The change of the Mercenary Review Board rating.</param>
/// <param name="ExperiencePerPilot">The experience every pilot of <see cref="Lance" /> gets.</param>
/// <param name="Lance">The company's mechs and pilots on the mission, in lance order.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MissionOutcome(
    string ModVersion,
    ExportTrigger Trigger,
    MissionContract Contract,
    BattleTech.Contract.ContractState State,
    bool IsGoodFaithEffort,
    IReadOnlyList<ObjectiveResult> Objectives,
    int Payment,
    ReputationChange Reputation,
    int MercenaryReviewBoardReputation,
    int ExperiencePerPilot,
    IReadOnlyList<LanceUnitOutcome> Lance,
    SalvageOffer Salvage) : ExportFile(ModVersion, null, Trigger);

/// <summary>The contract a mission was for, as in <see cref="Models.Contract" />.</summary>
/// <param name="Id">The contract's data file; <c>null</c> where the game keeps no reference to it.</param>
/// <param name="Type">The mission type, described in <c>rules.json</c>'s <see cref="Rules.ContractTypes" />.</param>
/// <param name="Difficulty">On the scale of <see cref="ReputationLevel.MaxContractDifficulty" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MissionContract(
    string? Id,
    string Name,
    DefinitionReference Type,
    ContractDisplayStyle DisplayStyle,
    DefinitionReference Employer,
    DefinitionReference Target,
    int Difficulty,
    DefinitionReference System);

/// <summary>An objective as the after-action report lists it.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ObjectiveResult(string Title, bool IsPrimary, ObjectiveStatus Status);

/// <summary>A mech and its pilot after the mission.</summary>
/// <param name="Injuries">The pilot's injuries, including those from before the mission.</param>
/// <param name="Health">The injuries the pilot can take before being incapacitated.</param>
/// <param name="IsKilled">Whether the pilot died; an incapacitated pilot may survive.</param>
/// <param name="PilotReadyOnDay">
///     The day the pilot is out of the med bay, as the barracks counts it once the contract is resolved; <c>null</c>
///     for an uninjured or killed pilot.
/// </param>
/// <param name="MechKills">The enemy mechs the pilot destroyed in this mission.</param>
/// <param name="OtherKills">The other enemy units the pilot destroyed in this mission.</param>
/// <param name="Mech">The mech, as in the mech bay.</param>
/// <param name="IsMechLost">
///     Whether the mech was destroyed and not recovered; its surviving components are in
///     <see cref="SalvageOffer.Automatic" />.
/// </param>
/// <param name="Repair">
///     What repairing the mech's damage in the mech bay would take, as for a mech bay mech's
///     <see cref="Mech.Repair" />; <c>null</c> without damage and for a lost mech.
/// </param>
/// <param name="DamagedLocations">The locations with structure damage, from head to legs; armor is repaired for free.</param>
/// <param name="DamagedComponents">The components damaged or destroyed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LanceUnitOutcome(
    PilotReference Pilot,
    int Injuries,
    int Health,
    bool IsIncapacitated,
    bool IsKilled,
    int? PilotReadyOnDay,
    int MechKills,
    int OtherKills,
    MechReference Mech,
    bool IsMechLost,
    RepairEstimate? Repair,
    IReadOnlyList<DamagedLocation> DamagedLocations,
    IReadOnlyList<DamagedComponent> DamagedComponents);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record DamagedLocation(ChassisLocations Location, Structure Structure);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record DamagedComponent(
    ComponentReference Component,
    ChassisLocations Location,
    ComponentDamageLevel DamageLevel);

/// <summary>The salvage of the mission, before the company chooses its priority salvage.</summary>
/// <param name="Total">How many items of <see cref="Pool" /> the company gets.</param>
/// <param name="Priority">
///     How many of them the company picks itself; the rest are drawn at random from what is left.
/// </param>
/// <param name="Pool">The items to choose from.</param>
/// <param name="Automatic">
///     The surviving components of the company's own lost mechs, which it gets back on top of <see cref="Total" />.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SalvageOffer(int Total, int Priority, SalvageItems Pool, SalvageItems Automatic);
