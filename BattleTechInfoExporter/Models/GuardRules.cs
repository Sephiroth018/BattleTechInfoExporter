using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     How guard levels reduce damage: bracing, Bulwark and cover from terrain each add a level. Attacks from
///     behind, melee and breaching shots ignore them.
/// </summary>
/// <param name="DamageMultiplierByLevel">Indexed by the guard level, from unguarded up.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record GuardRules(
    IReadOnlyList<float> DamageMultiplierByLevel,
    bool UnsteadyBreaksGuard,
    bool SolidDamageBreaksGuard,
    bool SensorLockBreaksGuard,
    bool CanVehiclesBeGuarded);
