using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A pilot in the barracks: the commander or a roster member.</summary>
/// <param name="DaysUntilReady">Zero when the pilot is <see cref="PilotStatus.Ready" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record BarracksPilot(
    string Id,
    string Name,
    string Callsign,
    PilotType Type,
    int Age,
    Gender Gender,
    string Expertise,
    Skills Skills,
    IReadOnlyList<DefinitionReference> Abilities,
    int Health,
    Experience Experience,
    int Injuries,
    PilotStatus Status,
    int DaysUntilReady,
    Spirits Spirits,
    ServiceRecord ServiceRecord) : Pilot(Id, Name, Callsign, Type, Age, Gender, Expertise, Skills, Abilities, Health);

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
