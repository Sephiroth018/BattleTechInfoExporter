using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>What injures a mech's pilot, besides head hits and knockdowns.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record InjuryRules(bool FromSideTorsoDestruction, bool FromAmmoExplosion, bool FromHeatShutdown);
