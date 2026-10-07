using System;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using UnityEngine;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds the rules file's campaign rules from the campaign constants (SimGameConstants), which the game has
///     already adjusted to the career's difficulty settings (SimGameDifficulty). Only constants that the game's
///     campaign code reads are exported; the ones it never reads, or reads only for presentation, are left out.
/// </summary>
internal static class CampaignRulesReader
{
    internal static CampaignRules Read(SimGameState simGame)
    {
        var constants = simGame.Constants;
        return new CampaignRules(
            ReadMechLab(constants),
            ReadMedBay(simGame),
            ReadHiring(simGame),
            ReadSalvage(constants),
            ReadFinances(simGame),
            ReadContracts(constants),
            new AllianceRules(
                Mathf.RoundToInt(constants.Story.AllyReputationThreshold),
                constants.Story.BreakAllianceReputationChange,
                constants.Story.AllianceBreakCooldown),
            new TravelRules(
                constants.Travel.DefaultFuelTime,
                constants.Travel.FuelStationFuelTime,
                constants.Travel.DefaultSystemTravelTime,
                constants.Travel.MaxJumpDistance));
    }

    // SimGameState.CreateMechRepairWorkOrder, CreateComponentRepairWorkOrder, CreateComponentInstallWorkOrder (which
    // picks the category by the component's type) and CreateMechArmorModifyWorkOrder; readying is ReadyMech's, the
    // refund GetWorkOrderRefundAmount's.
    private static MechLabRules ReadMechLab(SimGameConstants constants)
    {
        var mechLab = constants.MechLab;
        return new MechLabRules(
            new MechLabCost(mechLab.StructureRepairTechPoints, mechLab.StructureRepairCost),
            new MechLabCost(mechLab.ZeroStructureTechPointModifier, mechLab.ZeroStructureCBillModifier),
            new MechLabCost(mechLab.ComponentRepairTechPoints, mechLab.ComponentRepairCost),
            new InstallCosts(
                new MechLabCost(mechLab.BallisticInstallTechPoints, mechLab.BallisticInstallCost),
                new MechLabCost(mechLab.MissileInstallTechPoints, mechLab.MissileInstallCost),
                new MechLabCost(mechLab.EnergyInstallTechPoints, mechLab.EnergyInstallCost),
                new MechLabCost(mechLab.APInstallTechPoints, mechLab.APInstallCost),
                new MechLabCost(mechLab.AmmoInstallTechPoints, mechLab.AmmoInstallCost),
                new MechLabCost(mechLab.JumpJetInstallTechPoints, mechLab.JumpJetInstallCost),
                new MechLabCost(mechLab.HeatSinkInstallTechPoints, mechLab.HeatSinkInstallCost),
                new MechLabCost(mechLab.OtherInstallTechPoints, mechLab.OtherInstallCost)),
            new MechLabCost(mechLab.ArmorInstallTechPoints, mechLab.ArmorInstallCost),
            mechLab.UninstallTechPoints,
            constants.Story.MechReadyTime,
            constants.Finances.MechLabRefundModifier);
    }

    // SimGameState.GetDailyHealValue and GetInjuryCost; the death roll is Contract.FinalizeKilledMechWarriors'.
    private static MedBayRules ReadMedBay(SimGameState simGame)
    {
        var story = simGame.Constants.Story;
        var pilot = simGame.Constants.Pilot;
        return new MedBayRules(
            story.DailyHealValue,
            story.MedTechSkillMod,
            pilot.BaseInjuryDamageCost,
            pilot.LethalDamageCost,
            pilot.IncapacitatedDamageCost,
            new PilotDeathChance(pilot.IncapacitatedDeathChance, pilot.LethalDeathChance, pilot.GutsDeathReduction));
    }

    // SimGameState.GetMechWarriorHiringCost and GetMechWarriorValue; the hiring hall is StarSystem.GeneratePilots',
    // the health PilotGenerator.GenerateRandomPilot's, the limits CanMechWarriorBeHiredAccordingToMRBRating's and
    // CanMechWarriorBeHiredAccordingToMorale's.
    private static HiringRules ReadHiring(SimGameState simGame)
    {
        var constants = simGame.Constants;
        var story = constants.Story;
        return new HiringRules(
            constants.Finances.MechWarriorHiringCostPerPoint,
            constants.Finances.MechWarriorBaseCostPerPoint,
            constants.Finances.MechWarriorBonusCostPerPoint,
            story.DefaultPilotsPerSystem,
            story.DefaultRoninHiringChance,
            simGame.CombatConstants.PilotingConstants.DefaultMaxInjuries,
            // Level n starts at the (n - 1)th cap (SimGameState.GetMRBLevelFromRep); the limits and multipliers are
            // indexed by level, so only levels present in all three are complete.
            Enumerable.Range(
                    0,
                    Math.Min(
                        story.MRBRepCap.Length + 1,
                        Math.Min(story.MRBRepHiringPowerLevelLimits.Length, story.MRBRepMod.Length)))
                .Select(level => new MercenaryReviewBoardLevel(
                    level,
                    level == 0 ? 0 : Mathf.RoundToInt(story.MRBRepCap[level - 1]),
                    Mathf.RoundToInt(story.MRBRepHiringPowerLevelLimits[level]),
                    story.MRBRepMod[level]))
                .ToList(),
            // SimGameState.GetMoraleHiringLevelIndex picks the first threshold the morale is below.
            story.MoraleHiringThresholds
                .Take(story.MaxMoralePowerLevelLimits.Length)
                .Select((threshold, index) => new MoraleHiringLimit(
                    Mathf.RoundToInt(threshold),
                    Mathf.RoundToInt(story.MaxMoralePowerLevelLimits[index])))
                .ToList(),
            story.MaxMechWarriorsPerPod,
            story.MaxMechsPerPod);
    }

    // Contract.GenerateSalvage, AddWeaponToSalvage and AddMechComponentToSalvage; the mech's loadout is
    // SimGameState.AddMech's and SG_Shop_Screen's.
    private static SalvageRules ReadSalvage(SimGameConstants constants)
    {
        var salvage = constants.Salvage;
        return new SalvageRules(
            salvage.DefaultSalvagePotential,
            new SalvageOutcome(salvage.VictorySalvageChance, salvage.VictorySalvageLostPerMechDestroyed),
            new SalvageOutcome(salvage.RetreatSalvageChance, salvage.RetreatSalvageLostPerMechDestroyed),
            new SalvageOutcome(salvage.DefeatSalvageChance, salvage.DefeatSalvageLostPerMechDestroyed),
            salvage.PrioritySalvageModifier,
            ContractReader.MaxPrioritySalvage,
            constants.Finances.ContractFloorSalvageBonus,
            new SalvagePickWeights(
                salvage.DefaultWeaponWeight,
                salvage.DefaultComponentWeight,
                salvage.DefaultMechPartWeight),
            new RareSalvageSwaps(
                new RareSalvageSwap(
                    new RareSalvageTier(salvage.VeryRareWeaponLevel, salvage.VeryRareWeaponChance),
                    new RareSalvageTier(salvage.RareWeaponLevel, salvage.RareWeaponChance),
                    salvage.WeaponChanceDivisor),
                new RareSalvageSwap(
                    new RareSalvageTier(salvage.VeryRareUpgradeLevel, salvage.VeryRareUpgradeChance),
                    new RareSalvageTier(salvage.RareUpgradeLevel, salvage.RareUpgradeChance),
                    salvage.UpgradeChanceDivisor)),
            Mathf.RoundToInt(salvage.ItemAutoCullLevel),
            salvage.DestroyedMechRecoveryChance,
            salvage.EquipMechOnSalvage);
    }

    // SimGameState.GetExpenditures, GetShipBaseMaintenanceCost and GetExpenditureCostModifier; the jump cost is
    // SGTravelManager's, the shares Shop.GetAllInventoryShopItems' and SimGameState.ScrapActiveMech's, the debt
    // IsGameOverCondition's, the morale changes the ones SimGameState applies after a contract.
    private static FinancesRules ReadFinances(SimGameState simGame)
    {
        var finances = simGame.Constants.Finances;
        var story = simGame.Constants.Story;
        return new FinancesRules(
            finances.QuarterLength,
            finances.MechCostPerQuarter,
            new ShipMaintenance(finances.LeopardBaseMaintenanceCost, finances.ArgoBaseMaintenanceCost),
            simGame.ExpenditureMoraleValue
                .OrderBy(option => option.Key)
                .Select(option => new SpendingLevelRules(
                    option.Key,
                    simGame.GetExpenditureCostModifier(option.Key),
                    option.Value))
                .ToList(),
            finances.JumpShipCost,
            finances.ShopSellModifier,
            finances.ShopSellDamagedModifier,
            finances.MechScrapModifier,
            story.MaximumDebt,
            -story.BadFaithMoraleModifier,
            -story.CatastropheMoraleModifier);
    }

    // The pay is SimGameState.CalculateContractValueByContractType's, the payout Contract.CompleteContract's with
    // GetScaledCBillValue, the reputation Contract.GetBaseReputationValue's and GetNegotiableReputationBaseValue's,
    // the difficulty SimGameState.GetDifficultyRangeForContract's, the slots StarSystem.UpdateSystemDay's.
    private static ContractRules ReadContracts(SimGameConstants constants)
    {
        var finances = constants.Finances;
        var story = constants.Story;
        return new ContractRules(
            finances.ContractPricePerDifficulty,
            finances.ContractPriceVariance,
            finances.ContractFloorSalaryMultiplier,
            finances.GoodFaithModifier,
            finances.NoFaithModifier,
            new ContractReputationRules(
                finances.ContractBaseRepDifficultyMultiplier,
                finances.ContractBaseReputationAddition,
                finances.ContractNegotiableRepDifficultyMultiplier,
                finances.ContractNegotiableRepAddition,
                new OutcomeMultipliers(
                    story.EmployerRepSuccessMod,
                    story.EmployerRepGoodFaithMod,
                    story.EmployerRepBadFaithMod),
                new OutcomeMultipliers(story.TargetRepSuccessMod, story.TargetRepGoodFaithMod,
                    story.TargetRepBadFaithMod),
                new OutcomeMultipliers(story.MRBSuccessMod, story.MRBGoodFaithMod, story.MRBFailureMod)),
            new ContractExperienceRules(
                constants.Pilot.BaseXPGainPerMission,
                story.XPFailureMod,
                story.XPGoodFaithMod,
                story.XPBadFaithMod),
            Mathf.RoundToInt(story.GlobalContractDifficultyMax),
            story.ContractDifficultyVariance,
            story.MaxContractsPerSystem,
            story.ContractRenewalPerWeek,
            story.DefaultContractRefreshRate,
            story.ContractSuccessReduction);
    }
}
