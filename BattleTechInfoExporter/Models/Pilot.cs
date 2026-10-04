using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The fields every pilot has; the pilots of the barracks and of the hiring hall derive from it with fields of
///     their own.
/// </summary>
/// <param name="Name">The full name without callsign, as in <see cref="PilotReference" />.</param>
/// <param name="Expertise">The barracks title derived from the primary abilities, e.g. "Recruit" or "Lancer".</param>
/// <param name="Abilities">
///     The primary abilities only, described in <see cref="Rules.Skills" />; the passive traits follow from the
///     skill levels.
/// </param>
/// <param name="Health">The injuries the pilot can take before being incapacitated.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal record Pilot(
    string Id,
    string Name,
    string Callsign,
    PilotType Type,
    int Age,
    Gender Gender,
    string Expertise,
    Skills Skills,
    IReadOnlyList<DefinitionReference> Abilities,
    int Health);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Skills(int Gunnery, int Piloting, int Guts, int Tactics);
