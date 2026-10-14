using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>What allying with a faction takes and what breaking the alliance costs.</summary>
/// <param name="ReputationNeeded">The faction reputation an alliance needs.</param>
/// <param name="ReputationAfterBreaking">The faction reputation after breaking the alliance.</param>
/// <param name="CooldownDays">The days before a broken alliance can be renewed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record AllianceRules(int ReputationNeeded, int ReputationAfterBreaking, int CooldownDays);
