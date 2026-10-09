using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>Salvage items, counted like <see cref="Storage" />, where they end up.</summary>
/// <param name="Components">Their <see cref="StoredComponent.DamagedCount" /> is 0: the game generates no damaged salvage.</param>
/// <param name="MechParts">The mech parts, by the mech they assemble into.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SalvageItems(
    IReadOnlyList<StoredComponent> Components,
    IReadOnlyList<StoredMechParts> MechParts);
