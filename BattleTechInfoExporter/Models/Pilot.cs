using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A pilot in the barracks: the commander or a roster member.</summary>
/// <param name="Name">The full name without callsign, as in <see cref="PilotReference" />.</param>
/// <param name="Expertise">The barracks title derived from the primary abilities, e.g. "Recruit" or "Lancer".</param>
/// <param name="Abilities">
///     The primary abilities only, described in <see cref="Rules.Skills" />; the passive traits follow from the
///     skill levels.
/// </param>
/// <param name="Health">The injuries the pilot can take before being incapacitated.</param>
/// <param name="DaysUntilReady">Zero when the pilot is <see cref="PilotStatus.Ready" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Pilot(
    string Id,
    string Name,
    string Callsign,
    PilotType Type,
    int Age,
    Gender Gender,
    string Expertise,
    Skills Skills,
    Experience Experience,
    IReadOnlyList<DefinitionReference> Abilities,
    int Health,
    int Injuries,
    PilotStatus Status,
    int DaysUntilReady,
    Spirits Spirits,
    ServiceRecord ServiceRecord);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Skills(int Gunnery, int Piloting, int Guts, int Tactics);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Experience(int Unspent, int Spent);

/// <param name="DaysRemaining">Until high or low spirits end; <c>null</c> for normal spirits, which don't.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Spirits(SpiritsLevel Level, int? DaysRemaining);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ServiceRecord(
    int Missions,
    int MechKills,
    int OtherKills,
    int Ejections,
    int LifetimeInjuries,
    int DaysOnCrew);
