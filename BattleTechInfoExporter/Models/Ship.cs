using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The Argo and its upgrades, as the engineering screen shows them.</summary>
/// <param name="Upgrades">
///     The upgrades the engineering screen shows: installed, installing or available ones, and the locked ones they
///     lead to.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Ship(IReadOnlyList<ShipUpgrade> Upgrades);

/// <param name="Category">The engineering screen's group, e.g. "Mech Bay".</param>
/// <param name="Details">The upgrade's description, which also names effects the stats don't, e.g. training.</param>
/// <param name="RequiredUpgrades">The upgrades that must be installed before this one can be bought.</param>
/// <param name="PurchaseCost">The one-time C-Bills to buy the upgrade.</param>
/// <param name="Upkeep">
///     What the upgrade adds to the expenses of each report once installed; an installed upgrade's line in
///     <see cref="ExpectedExpenses.ShipUpgrades" /> is the same.
/// </param>
/// <param name="InstallDays">The days the upgrade takes to install.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ShipUpgrade(
    DefinitionReference Upgrade,
    ShipUpgradeStatus Status,
    DefinitionReference Category,
    DropshipLocation Location,
    string Details,
    IReadOnlyList<DefinitionReference> RequiredUpgrades,
    int PurchaseCost,
    int Upkeep,
    int InstallDays,
    IReadOnlyList<ShipUpgradeEffect> Effects);

/// <summary>A change the upgrade makes to a company statistic once installed.</summary>
/// <param name="Statistic">The game's name of the company statistic, e.g. "MechTechSkill".</param>
/// <param name="IsSet">Whether the upgrade sets the statistic to <see cref="Value" /> instead of adding it.</param>
/// <param name="Description">The effect in the game's words, e.g. "+2 Morale".</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ShipUpgradeEffect(string Statistic, float Value, bool IsSet, string Description);
