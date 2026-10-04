using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A pilot for hire in the current system's hiring hall.</summary>
/// <param name="HireCost">The one-time C-Bills to hire the pilot.</param>
/// <param name="Salary">What the pilot would add to the expenses of each report, as a crew member's line does.</param>
/// <param name="CanHire">
///     Whether the company's Mercenary Review Board rating and morale allow hiring the pilot; full barracks
///     (<see cref="Company.MaxPilots" />), travel and funds can block it as well.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HiringHallPilot(
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
    int HireCost,
    int Salary,
    bool CanHire) : Pilot(Id, Name, Callsign, Type, Age, Gender, Expertise, Skills, Abilities, Health);
