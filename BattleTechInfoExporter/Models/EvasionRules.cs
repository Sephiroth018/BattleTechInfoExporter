using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>How a unit earns evasive pips by moving and what each pip does to ranged attacks against it.</summary>
/// <param name="PipsByDistanceMoved">
///     The pips for moving more than a distance, before a jump's extra pip and the unit's own maximum.
/// </param>
/// <param name="ModifierByPips">
///     The to-hit modifier by the number of pips, from one pip up; each pip beyond the last entry adds the
///     difference of the last two.
/// </param>
/// <param name="WeaponTypesAffected">The weapon types the pips apply to, by the game's names.</param>
/// <param name="VehiclePipMultiplier">A vehicle's pips are multiplied by this and rounded down.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EvasionRules(
    IReadOnlyList<EvasivePips> PipsByDistanceMoved,
    IReadOnlyList<float> ModifierByPips,
    bool JumpAddsPip,
    int SensorLockStripsPips,
    IReadOnlyList<WeaponType> WeaponTypesAffected,
    float VehiclePipMultiplier,
    bool CanVehiclesBeEvasive);

/// <param name="MoreThan">The distance in meters the unit has to exceed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EvasivePips(float MoreThan, int Pips);
