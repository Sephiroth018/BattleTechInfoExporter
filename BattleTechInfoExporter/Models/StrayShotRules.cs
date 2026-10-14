using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A missed ranged shot hits the first building in its path within the weapon's maximum range instead, or a
///     unit where <see cref="UnitsHit" /> allows; never the attacker, and the target only when it's a building.
/// </summary>
/// <param name="UnitsHit">
///     Which units a missed shot can hit, from the attacker's side: <c>ENEMIES_ONLY</c>, <c>ENEMIES_AND_NEUTRAL</c>
///     or <c>EVERYONE</c>; <c>null</c> when it hits only buildings.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StrayShotRules(StrayShotValidTargets? UnitsHit);
