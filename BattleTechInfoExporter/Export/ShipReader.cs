using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BattleTech;
using BattleTech.UI.Tooltips;
using BattleTechInfoExporter.Models;
using UnityEngine;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the Argo's upgrades from the game's career state.</summary>
internal static class ShipReader
{
    // TextMeshPro's formatting tags, e.g. <i> or <color=#F04228>, which the game's texts carry for its UI.
    private static readonly Regex RichTextTag = new("</?[a-zA-Z][^>]*>", RegexOptions.Compiled);

    // On the Leopard the starting upgrades are already listed, but the game applies them and shows the engineering
    // screen only on the Argo (SimGameState.AddArgoUpgrade, ApplyArgoUpgrades).
    internal static Ship? ReadShip(SimGameState simGame) =>
        simGame.CurDropship == DropshipType.Argo
            ? new Ship(
                simGame.ShipUpgrades
                    .Select(upgrade => ReadUpgrade(simGame, upgrade))
                    .OrderByDefinition(upgrade => upgrade.Upgrade)
                    .ToList(),
                ReadAvailableUpgrades(simGame))
            : null;

    // Mirrors the available upgrades of SGEngineeringScreen.PopulateUpgradeDictionary.
    private static List<AvailableShipUpgrade> ReadAvailableUpgrades(SimGameState simGame) =>
        simGame.DataManager.ResourceLocator.AllEntriesOfResource(BattleTechResourceType.ShipModuleUpgrade)
            .Select(entry => simGame.DataManager.ShipUpgradeDefs.Get(entry.Id))
            .Where(upgrade => !simGame.HasShipUpgrade(upgrade.Description.Id)
                              && !simGame.UpgradeInProgress(upgrade.Description.Id)
                              && simGame.HasShipUpgrade(upgrade.RequiredModules))
            .Select(upgrade => ReadAvailableUpgrade(simGame, upgrade))
            .OrderByDefinition(upgrade => upgrade.Upgrade)
            .ToList();

    // The values SGShipModuleUpgradeViewPopulator.Populate shows, which QueueArgoUpgrade charges.
    private static AvailableShipUpgrade ReadAvailableUpgrade(SimGameState simGame, ShipModuleUpgrade upgrade) =>
        new(ReadUpgrade(simGame, upgrade),
            upgrade.RequiredModules
                .Select(id => DefinitionReferences.ReferenceTo(simGame.DataManager.ShipUpgradeDefs.Get(id).Description))
                .OrderByDefinition(reference => reference)
                .ToList(),
            Mathf.CeilToInt(upgrade.PurchaseCost * simGame.Constants.CareerMode.ArgoUpgradeCostMultiplier),
            FinancesReader.ReadUpkeep(simGame, upgrade),
            upgrade.TechCost / simGame.DailyUpgradeValue);

    private static ShipUpgrade ReadUpgrade(SimGameState simGame, ShipModuleUpgrade upgrade) =>
        new(DefinitionReferences.ReferenceTo(upgrade.Description),
            DefinitionReferences.ReferenceTo(upgrade.ShipUpgradeCategoryValue),
            upgrade.Location,
            WithoutRichTextTags(upgrade.Description.Details),
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
            : new ShipUpgradeEffect(stat.name, stat.ToSingle(), stat.set, PlainText(description));
    }

    // The text without its tooltip links, as LocalizableText shows it with links disabled, and without formatting.
    private static string PlainText(ResultDescriptionEntry description)
    {
        var parser = new TextTooltipParser();
        parser.Parse(description.Text);
        parser.SetFormattingEnabled(false);
        return WithoutRichTextTags(parser.ToTMP(description.Context, null).ToString()).Trim();
    }

    private static string WithoutRichTextTags(string text) => RichTextTag.Replace(text, string.Empty);
}
