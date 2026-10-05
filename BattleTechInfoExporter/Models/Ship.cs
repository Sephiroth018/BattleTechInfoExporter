using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The Argo and its upgrades, as the engineering screen shows them.</summary>
/// <param name="AvailableUpgrades">
///     The upgrades whose required upgrades are all installed, without the one being installed. While one is being
///     installed, no other can be bought; funds can block buying one as well.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Ship(
    IReadOnlyList<ShipUpgrade> InstalledUpgrades,
    IReadOnlyList<AvailableShipUpgrade> AvailableUpgrades);

/// <summary>
///     The fields every ship upgrade has; the available upgrades derive from it with what buying one takes.
/// </summary>
/// <param name="Category">The engineering screen's group, e.g. "Mech Bay".</param>
/// <param name="Details">The upgrade's description, which also names effects the stats don't, e.g. training.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal record ShipUpgrade(
    DefinitionReference Upgrade,
    DefinitionReference Category,
    DropshipLocation Location,
    string Details,
    IReadOnlyList<ShipUpgradeEffect> Effects);

/// <summary>A change the upgrade makes to a company statistic once installed.</summary>
/// <param name="Statistic">The game's name of the company statistic, e.g. "MechTechSkill".</param>
/// <param name="IsSet">Whether the upgrade sets the statistic to <see cref="Value" /> instead of adding it.</param>
/// <param name="Description">The effect in the game's words, e.g. "+2 Morale".</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ShipUpgradeEffect(string Statistic, float Value, bool IsSet, string Description);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record AvailableShipUpgrade : ShipUpgrade
{
    internal AvailableShipUpgrade(
        ShipUpgrade upgrade,
        IReadOnlyList<DefinitionReference> requiredUpgrades,
        int purchaseCost,
        int upkeep,
        int installDays) : base(upgrade)
    {
        RequiredUpgrades = requiredUpgrades;
        PurchaseCost = purchaseCost;
        Upkeep = upkeep;
        InstallDays = installDays;
    }

    /// <summary>The installed upgrades this one builds on.</summary>
    public IReadOnlyList<DefinitionReference> RequiredUpgrades { get; }

    /// <summary>The one-time C-Bills to buy the upgrade.</summary>
    public int PurchaseCost { get; }

    /// <summary>What the upgrade would add to the expenses of each report, as an installed upgrade's line does.</summary>
    public int Upkeep { get; }

    /// <summary>The days the upgrade takes to install.</summary>
    public int InstallDays { get; }
}
