using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     What the next report deducts if nothing changes until then. The lines are rounded one by one like
///     on the game's finance screen, so they can differ from <see cref="Total" /> by a few C-Bills.
/// </summary>
/// <param name="Total">The C-Bills the report deducts at the current spending level.</param>
/// <param name="Ship">The ship's base upkeep, without its upgrades.</param>
/// <param name="ShipUpgrades">
///     The upkeep of each ship upgrade that has any; empty before the company has the Argo.
/// </param>
/// <param name="Mechs">The upkeep of each mech in the mech bay.</param>
/// <param name="Pilots">The salary of each pilot in the barracks but the commander, who draws none.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ExpectedExpenses(
    int Total,
    ShipExpense Ship,
    IReadOnlyList<ShipUpgradeExpense> ShipUpgrades,
    IReadOnlyList<MechExpense> Mechs,
    IReadOnlyList<PilotExpense> Pilots);

/// <param name="Name">
///     The line's name on the finance screen: the Argo's operating costs, or the bank loan interest before the
///     company has the Argo.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ShipExpense(string Name, int Amount) : IExpense;

/// <param name="Upgrade">The ship upgrade whose upkeep the line is.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ShipUpgradeExpense(DefinitionReference Upgrade, int Amount) : IExpense;

/// <param name="Mech">The mech in the mech bay whose upkeep the line is.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechExpense(MechReference Mech, int Amount) : IExpense;

/// <param name="Pilot">The pilot whose salary the line is.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record PilotExpense(PilotReference Pilot, int Amount) : IExpense;
