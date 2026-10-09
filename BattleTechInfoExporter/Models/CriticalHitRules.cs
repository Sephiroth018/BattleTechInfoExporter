using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     Critical hits on a mech's components, rolled when a shot damages a location's structure: the chance is the
///     location's share of lost structure, at least <see cref="MinChance" />, times the weapon's multiplier.
/// </summary>
/// <param name="MinChance">As a fraction from 0 to 1.</param>
/// <param name="AiAttackerMultiplier">Multiplies the chance when the attacker isn't player-controlled.</param>
/// <param name="EmptySlotsAbsorbCrits">Whether a critical hit on an empty or destroyed slot does nothing.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CriticalHitRules(float MinChance, float AiAttackerMultiplier, bool EmptySlotsAbsorbCrits);
