using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     How guard levels reduce damage: bracing, Bulwark and cover from terrain each add a level. Attacks from
///     behind, melee and breaching shots ignore them.
/// </summary>
/// <param name="DamageMultiplierByLevel">Indexed by the guard level, from unguarded up.</param>
/// <param name="UnsteadyBreaksGuard">
///     Whether an unsteady unit loses the levels from bracing and Bulwark; cover still counts.
/// </param>
/// <param name="SolidDamageBreaksGuard">
///     Whether a mech hit without guard since its last activation loses the levels from bracing and Bulwark.
/// </param>
/// <param name="SensorLockBreaksGuard">
///     Whether a sensor-locked unit loses the levels from bracing and Bulwark.
/// </param>
/// <param name="CanVehiclesBeGuarded">Whether vehicles get every guard level; when not, they get one at most.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record GuardRules(
    IReadOnlyList<float> DamageMultiplierByLevel,
    bool UnsteadyBreaksGuard,
    bool SolidDamageBreaksGuard,
    bool SensorLockBreaksGuard,
    bool CanVehiclesBeGuarded);
