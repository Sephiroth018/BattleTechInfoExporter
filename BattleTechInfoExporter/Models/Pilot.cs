using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The fields every pilot has; the pilots of the barracks and of the hiring hall derive from it with fields of
///     their own.
/// </summary>
/// <param name="Id">The game's pilot id, as in <see cref="PilotReference" />.</param>
/// <param name="Name">The full name without callsign, as in <see cref="PilotReference" />.</param>
/// <param name="Callsign">The callsign, which generated pilots can share.</param>
/// <param name="Type">The kind of pilot, which the barracks tells apart by color.</param>
/// <param name="Age">The age in years.</param>
/// <param name="Gender">The game's gender value, e.g. <c>Female</c>.</param>
/// <param name="Expertise">The barracks title derived from the primary abilities, e.g. "Recruit" or "Lancer".</param>
/// <param name="Skills">The skill levels, whose effects <see cref="Rules.Skills" /> lists per level.</param>
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

/// <summary>A pilot's skill levels, each from 1 to 10.</summary>
/// <param name="Gunnery">The gunnery level, a level of <see cref="SkillRules.Gunnery" />.</param>
/// <param name="Piloting">The piloting level, a level of <see cref="SkillRules.Piloting" />.</param>
/// <param name="Guts">The guts level, a level of <see cref="SkillRules.Guts" />.</param>
/// <param name="Tactics">The tactics level, a level of <see cref="SkillRules.Tactics" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Skills(int Gunnery, int Piloting, int Guts, int Tactics);
