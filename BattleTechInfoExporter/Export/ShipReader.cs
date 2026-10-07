using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using UnityEngine;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the Argo's upgrades from the game's career state.</summary>
internal static class ShipReader
{
    // On the Leopard the starting upgrades are already listed, but the game applies them and shows the engineering
    // screen only on the Argo (SimGameState.AddArgoUpgrade, ApplyArgoUpgrades).
    internal static Ship? ReadShip(SimGameState simGame) =>
        simGame.CurDropship == DropshipType.Argo
            ? new Ship(ReadShownUpgrades(simGame).OrderByDefinition(upgrade => upgrade.Upgrade).ToList())
            : null;

    // Mirrors SGEngineeringScreen.PopulateUpgradeDictionary: the installed, installing and available upgrades,
    // then the locked ones whose required upgrades are all among them; the screen shows no others.
    private static IEnumerable<ShipUpgrade> ReadShownUpgrades(SimGameState simGame)
    {
        var definitions = simGame.DataManager.ShipUpgradeDefs;
        var upgrades = definitions.Keys.Select(definitions.Get).ToList();
        var unlocked = new List<(ShipModuleUpgrade Upgrade, ShipUpgradeStatus Status)>();
        foreach (var upgrade in upgrades)
        {
            if (UnlockedStatusOf(simGame, upgrade) is { } status)
            {
                unlocked.Add((upgrade, status));
            }
        }

        var unlockedIds = unlocked.Select(entry => entry.Upgrade.Description.Id).ToList();
        var locked = upgrades
            .Where(upgrade => !unlockedIds.Contains(upgrade.Description.Id)
                              && simGame.HasShipUpgrade(upgrade.RequiredModules, unlockedIds))
            .Select(upgrade => ReadUpgrade(simGame, upgrade, ShipUpgradeStatus.Locked));
        return unlocked.Select(entry => ReadUpgrade(simGame, entry.Upgrade, entry.Status)).Concat(locked);
    }

    private static ShipUpgradeStatus? UnlockedStatusOf(SimGameState simGame, ShipModuleUpgrade upgrade)
    {
        var id = upgrade.Description.Id;
        if (simGame.HasShipUpgrade(id))
        {
            return ShipUpgradeStatus.Installed;
        }

        if (simGame.UpgradeInProgress(id))
        {
            return ShipUpgradeStatus.Installing;
        }

        return simGame.HasShipUpgrade(upgrade.RequiredModules) ? ShipUpgradeStatus.Available : null;
    }

    // The values SGShipModuleUpgradeViewPopulator.Populate shows; QueueArgoUpgrade charges the same.
    private static ShipUpgrade ReadUpgrade(SimGameState simGame, ShipModuleUpgrade upgrade, ShipUpgradeStatus status) =>
        new(DefinitionReferences.ReferenceTo(upgrade.Description),
            status,
            DefinitionReferences.ReferenceTo(upgrade.ShipUpgradeCategoryValue),
            upgrade.Location,
            GameText.ToPlainText(upgrade.Description.Details),
            upgrade.RequiredModules
                .Select(id => DefinitionReferences.ReferenceTo(simGame.DataManager.ShipUpgradeDefs.Get(id).Description))
                .OrderByDefinition(reference => reference)
                .ToList(),
            Mathf.CeilToInt(upgrade.PurchaseCost * simGame.Constants.CareerMode.ArgoUpgradeCostMultiplier),
            FinancialReportReader.ReadUpkeep(simGame, upgrade),
            upgrade.TechCost / simGame.DailyUpgradeValue,
            upgrade.Stats
                .Select(stat => ReadEffect(simGame, stat))
                .OfType<ShipUpgradeEffect>()
                .ToList());

    // The effect text of SGShipModuleUpgradeViewPopulator.BuildEffectsString, one stat at a time; a stat without
    // one, e.g. because its description is hidden, is left out as on the screen.
    private static ShipUpgradeEffect? ReadEffect(SimGameState simGame, SimGameStat stat)
    {
        var description = simGame
            .BuildSimGameStatsResults([stat], simGame.Context, SimGameStatDescDef.DescriptionTense.Infinitive,
                string.Empty)
            .SingleOrDefault();
        return description is null
            ? null
            : new ShipUpgradeEffect(stat.name, stat.ToSingle(), stat.set, GameText.ToPlainText(description));
    }
}
