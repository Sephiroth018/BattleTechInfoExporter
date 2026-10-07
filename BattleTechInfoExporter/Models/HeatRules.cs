using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The heat rules of mechs; vehicles and turrets have no heat. What Coolant Vent does is with the ability in
///     <see cref="SkillRules" />.
/// </summary>
/// <param name="MaxHeat">The heat at which a mech shuts down.</param>
/// <param name="OverheatsAbove">
///     Above this heat the mech is overheated: it takes <see cref="OverheatStructureDamageByWeightClass" /> in every
///     location but the head whenever its heat is reconciled, and its attacks and movement are penalized.
/// </param>
/// <param name="EngineHeatSinks">The heat sinks every mech has built in, on top of the chassis' dissipation.</param>
/// <param name="MaxHeatAfterRestart">A mech restarting from shutdown is cooled to at most this heat.</param>
/// <param name="HeatGeneratedMultiplier">Multiplies all heat a mech generates itself.</param>
/// <param name="HeatSinkMultiplier">Multiplies a mech's heat sink capacity, together with the terrain's and biome's.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HeatRules(
    int MaxHeat,
    int OverheatsAbove,
    int WalkHeat,
    int SprintHeat,
    JumpHeat JumpHeat,
    int EngineDamageHeat,
    int EngineHeatSinks,
    float EngineHeatSinkDissipation,
    int MaxHeatAfterRestart,
    float HeatGeneratedMultiplier,
    float HeatSinkMultiplier,
    ByWeightClass<float> OverheatStructureDamageByWeightClass);

/// <summary>
///     A jump's heat: <see cref="PerUnit" /> for each started <see cref="UnitSize" /> meters of the distance, at
///     least <see cref="Minimum" />.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record JumpHeat(float UnitSize, int PerUnit, int Minimum);
