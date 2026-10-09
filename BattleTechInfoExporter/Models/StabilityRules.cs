using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The stability rules of mechs. A mech's stability bar has <see cref="Levels" /> levels; a mech that reaches
///     <see cref="UnsteadyThresholdPercent" /> of its stability is unsteady and loses its evasive pips, and an
///     unsteady mech that fills the bar is knocked down.
/// </summary>
/// <param name="UnsteadyThresholdPercent">
///     In percent of the stability bar; a mech's <c>UnsteadyThreshold</c> statistic starts at it.
/// </param>
/// <param name="Levels">The number of equal levels the stability bar is divided into.</param>
/// <param name="LevelsRecovered">
///     The levels of instability an action removes, after the level the mech is in is always emptied.
/// </param>
/// <param name="LevelsAdded">
///     The levels of instability an action adds to the mech doing it, after the level the mech is in is emptied.
/// </param>
/// <param name="EntrenchedInstabilityMultiplier">Multiplies the instability a braced mech takes from weapons.</param>
/// <param name="MinStabilityPerDestroyedLocation">
///     The share of the stability bar a mech can't recover below for each destroyed location.
/// </param>
/// <param name="OnlyLegsRaiseMinStability">Whether only destroyed legs count for it.</param>
/// <param name="InstabilityFromDamage">
///     The share of the stability bar added when a location is damaged, on top of a weapon's own instability.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StabilityRules(
    int UnsteadyThresholdPercent,
    int Levels,
    LevelsRecoveredByAction LevelsRecovered,
    LevelsAddedByAction LevelsAdded,
    float EntrenchedInstabilityMultiplier,
    float MinStabilityPerDestroyedLocation,
    bool OnlyLegsRaiseMinStability,
    InstabilityFromDamage InstabilityFromDamage);

/// <param name="Stationary">When the mech moved less than a meter this round, or not at all.</param>
/// <param name="Walked">When the mech moved without sprinting or jumping; a sprint recovers nothing.</param>
/// <param name="Jumped">When the mech jumped.</param>
/// <param name="StoodUp">When the mech stood up.</param>
/// <param name="Braced">When the mech braced.</param>
/// <param name="Deferred">When the mech's pilot has the ability that resets instability on deferring.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LevelsRecoveredByAction(
    int Stationary,
    int Walked,
    int Jumped,
    int StoodUp,
    int Braced,
    int Deferred);

/// <param name="DeathFromAbove">When the mech's own jump attack makes it unsteady.</param>
/// <param name="Falling">When the building the mech stands on is destroyed and it falls.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LevelsAddedByAction(int DeathFromAbove, int Falling);

/// <param name="SideTorsoDestroyed">Added when a side torso is destroyed.</param>
/// <param name="CenterTorsoDamaged">Added when the center torso's damage reaches each of its two damage levels.</param>
/// <param name="ArmDestroyed">Added when an arm is destroyed.</param>
/// <param name="LegDamaged">Added on any leg damage; a destroyed leg fills the bar instead.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record InstabilityFromDamage(
    float SideTorsoDestroyed,
    float CenterTorsoDamaged,
    float ArmDestroyed,
    float LegDamaged);
