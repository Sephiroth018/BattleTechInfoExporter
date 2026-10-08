using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     What the next report deducts if nothing changes until then. The lines are rounded one by one like
///     on the game's finance screen, so they can differ from <see cref="Total" /> by a few C-Bills.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ExpectedExpenses(
    int Total,
    ShipExpense Ship,
    IReadOnlyList<ShipUpgradeExpense> ShipUpgrades,
    IReadOnlyList<MechExpense> Mechs,
    IReadOnlyList<PilotExpense> Pilots);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ShipExpense(string Name, int Amount) : IExpense;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ShipUpgradeExpense(DefinitionReference Upgrade, int Amount) : IExpense;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechExpense(MechReference Mech, int Amount) : IExpense;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record PilotExpense(PilotReference Pilot, int Amount) : IExpense;
