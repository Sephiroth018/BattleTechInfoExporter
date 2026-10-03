using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using Localize;
using UnityEngine;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the export's model from the game's career state.</summary>
internal static class GameStateReader
{
    internal static GameState Read(SimGameState simGame, ExportTrigger trigger) =>
        new(
            ModAssembly.Version,
            DateTimeOffset.Now,
            trigger,
            ReadCompany(simGame),
            ReadPosition(simGame),
            ReadMoraleLevels(simGame));

    private static Company ReadCompany(SimGameState simGame) =>
        new(
            simGame.CompanyName,
            simGame.CurDropship,
            simGame.DaysPassed,
            simGame.CurrentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            new Morale(simGame.Morale, simGame.GetCurrentMoraleLevelDescriptor()),
            ReadFinances(simGame),
            new MercenaryReviewBoard(
                simGame.GetRawReputation(FactionEnumeration.GetMercenaryReviewBoardFactionValue()),
                simGame.GetCurrentMRBLevel()),
            // The game models the Mercenary Review Board as a faction, but it isn't one; it has its own object.
            FactionEnumeration.FactionList
                .Where(faction => faction.DoesGainReputation && !faction.IsMercenaryReviewBoard)
                .Select(faction => ReadReputation(simGame, faction))
                .ToList(),
            simGame.MechTechSkill,
            simGame.MedTechSkill);

    private static Finances ReadFinances(SimGameState simGame) =>
        new(
            simGame.Funds,
            simGame.DayRemainingInQuarter,
            new Spending(
                simGame.ExpenditureLevel,
                simGame.ExpenditureMoraleValue
                    .Select(option => new SpendingOption(option.Key, simGame.GetExpenditures(option.Key), option.Value))
                    .ToList()),
            ReadExpectedExpenses(simGame));

    // Mirrors the line items of SGCaptainsQuartersStatusScreen.RefreshData, including its rounding;
    // the game has no method that returns them.
    private static ExpectedExpenses ReadExpectedExpenses(SimGameState simGame)
    {
        var costModifier = simGame.GetExpenditureCostModifier(simGame.ExpenditureLevel);
        var shipName = simGame.CurDropship == DropshipType.Leopard
            ? Strings.T("Bank Loan Interest Payment")
            : Strings.T("Argo Operating Costs");
        return new ExpectedExpenses(
            simGame.GetExpenditures(),
            new ShipExpense(shipName, Mathf.RoundToInt(costModifier * simGame.GetShipBaseMaintenanceCost())),
            ReadShipUpgradeExpenses(simGame, costModifier),
            simGame.ActiveMechs.Values
                .Select(mech => new MechExpense(
                    new DefinitionReference(mech.Description.Id, mech.Name),
                    Mathf.RoundToInt(costModifier * simGame.Constants.Finances.MechCostPerQuarter)))
                .ToList(),
            simGame.PilotRoster
                .Select(pilot => new PilotExpense(
                    new DefinitionReference(pilot.pilotDef.Description.Id, pilot.pilotDef.Description.DisplayName),
                    Mathf.CeilToInt(costModifier * simGame.GetMechWarriorValue(pilot.pilotDef))))
                .ToList());
    }

    // Only the Argo charges upkeep for its upgrades.
    private static List<ShipUpgradeExpense> ReadShipUpgradeExpenses(SimGameState simGame, float costModifier)
    {
        if (simGame.CurDropship != DropshipType.Argo)
        {
            return [];
        }

        return simGame.ShipUpgrades
            .Select(upgrade => (upgrade, upkeep: Mathf.CeilToInt(
                upgrade.AdditionalCost * simGame.Constants.CareerMode.ArgoMaintenanceMultiplier)))
            .Where(upgradeUpkeep => upgradeUpkeep.upkeep > 0)
            .Select(upgradeUpkeep => new ShipUpgradeExpense(
                ReferenceTo(upgradeUpkeep.upgrade.Description),
                Mathf.RoundToInt(costModifier * upgradeUpkeep.upkeep)))
            .ToList();
    }

    private static List<MoraleLevel> ReadMoraleLevels(SimGameState simGame)
    {
        var moraleConstants = simGame.CombatConstants.MoraleConstants;
        return simGame.Constants.Story.MoraleLevelNames
            .Select((name, level) => new MoraleLevel(
                name,
                moraleConstants.BaselineAddFromSimGameThresholds[level],
                moraleConstants.BaselineAddFromSimGameValues[level]))
            .ToList();
    }

    private static FactionReputation ReadReputation(SimGameState simGame, FactionValue faction) =>
        new(
            ReferenceTo(faction),
            simGame.GetRawReputation(faction),
            simGame.GetReputation(faction),
            simGame.IsFactionAlly(faction),
            simGame.IsFactionEnemy(faction),
            simGame.displayedFactions.Contains(faction.Name));

    private static Position ReadPosition(SimGameState simGame) =>
        new(
            ReferenceTo(simGame.CurSystem.Def.Description),
            ReferenceTo(simGame.CurSystem.OwnerValue),
            simGame.TravelState,
            ReadTravel(simGame));

    private static Travel? ReadTravel(SimGameState simGame)
    {
        var destination = simGame.Starmap.Destination?.System;
        return simGame.TravelState == SimGameTravelStatus.IN_SYSTEM || destination is null
            ? null
            : new Travel(ReferenceTo(destination.Def.Description), simGame.TravelTime);
    }

    private static DefinitionReference ReferenceTo(BaseDescriptionDef description) =>
        new(description.Id, description.Name);

    private static DefinitionReference ReferenceTo(FactionValue faction) =>
        new(faction.FactionDefID, FactionNames.Format(faction));
}
