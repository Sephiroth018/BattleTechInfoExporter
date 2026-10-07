using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     How long healing takes: a pilot's injuries cost heal points, and the med bay pays
///     <see cref="HealPointsBase" /> plus <see cref="HealPointsPerMedTech" /> times the company's
///     <see cref="Company.MedTech" /> a day to each injured pilot.
/// </summary>
/// <param name="InjuryCost">Divided by the pilot's health for each injury, rounded down.</param>
/// <param name="LethalInjuryCost">Added once for a pilot who took lethal injuries and survived.</param>
/// <param name="IncapacitatedCost">Added once for a pilot who was incapacitated.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MedBayRules(
    int HealPointsBase,
    int HealPointsPerMedTech,
    int InjuryCost,
    int LethalInjuryCost,
    int IncapacitatedCost,
    PilotDeathChance DeathChance);

/// <summary>
///     The chance that an incapacitated pilot dies after the mission: <see cref="Incapacitated" />, or
///     <see cref="Lethal" /> after lethal injuries, less <see cref="ReductionPerGuts" /> per point of Guts.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record PilotDeathChance(float Incapacitated, float Lethal, float ReductionPerGuts);
