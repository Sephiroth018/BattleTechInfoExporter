using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using Localize;
using UnityEngine;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the company's finances and the expense lines of each report from the game's career state.</summary>
internal static class FinancesReader
{
    internal static Finances ReadFinances(SimGameState simGame) =>
        new(
            simGame.Funds,
            simGame.DayRemainingInQuarter,
            new Spending(
                simGame.ExpenditureLevel,
                simGame.ExpenditureMoraleValue
                    .Select(option => new SpendingOption(option.Key, simGame.GetExpenditures(option.Key), option.Value))
                    .ToList()),
            ReadExpectedExpenses(simGame));

    /// <summary>
    ///     The pilot's line in the expenses of each report, with the spending level's cost modifier
    ///     (SimGameState.GetExpenditureCostModifier).
    /// </summary>
    /// <remarks>
    ///     Rounded as SGCaptainsQuartersStatusScreen.RefreshData does; the hiring hall rounds to the nearest instead
    ///     (SimGameState.GetMechWarriorSalary).
    /// </remarks>
    internal static int ReadSalary(SimGameState simGame, PilotDef pilot) =>
        Mathf.CeilToInt(CostModifierOf(simGame) * simGame.GetMechWarriorValue(pilot));

    // Mirrors the line items of SGCaptainsQuartersStatusScreen.RefreshData, including its rounding;
    // the game has no method that returns them.
    private static ExpectedExpenses ReadExpectedExpenses(SimGameState simGame)
    {
        var costModifier = CostModifierOf(simGame);
        var shipName = simGame.CurDropship == DropshipType.Leopard
            ? Strings.T("Bank Loan Interest Payment")
            : Strings.T("Argo Operating Costs");
        return new ExpectedExpenses(
            simGame.GetExpenditures(),
            new ShipExpense(shipName, Mathf.RoundToInt(costModifier * simGame.GetShipBaseMaintenanceCost())),
            ReadShipUpgradeExpenses(simGame, costModifier),
            simGame.ActiveMechs.Values
                .Select(mech => new MechExpense(
                    MechReader.ReferenceToBayMech(mech),
                    Mathf.RoundToInt(costModifier * simGame.Constants.Finances.MechCostPerQuarter)))
                .ToList(),
            simGame.PilotRoster
                .Select(pilot => new PilotExpense(
                    PilotReader.ReferenceTo(pilot),
                    ReadSalary(simGame, pilot.pilotDef)))
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
                DefinitionReferences.ReferenceTo(upgradeUpkeep.upgrade.Description),
                Mathf.RoundToInt(costModifier * upgradeUpkeep.upkeep)))
            .ToList();
    }

    private static float CostModifierOf(SimGameState simGame) =>
        simGame.GetExpenditureCostModifier(simGame.ExpenditureLevel);
}
