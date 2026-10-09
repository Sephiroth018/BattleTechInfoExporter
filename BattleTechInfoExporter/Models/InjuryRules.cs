using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>What injures a mech's pilot, besides head hits and knockdowns.</summary>
/// <param name="FromSideTorsoDestruction">Whether the destruction of a side torso injures the pilot.</param>
/// <param name="FromAmmoExplosion">Whether an exploding ammunition box or other component injures the pilot.</param>
/// <param name="FromHeatShutdown">Whether shutting down from heat injures the pilot.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record InjuryRules(bool FromSideTorsoDestruction, bool FromAmmoExplosion, bool FromHeatShutdown);
