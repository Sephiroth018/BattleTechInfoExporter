using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using Localize;
using UnityEngine;
using Pilot = BattleTechInfoExporter.Models.Pilot;

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
            ReadPilots(simGame),
            MechReader.ReadMechs(simGame),
            ReadPosition(simGame),
            new Rules(ReadMoraleLevels(simGame), ReadReputationLevels(simGame), ReadSkillRules(simGame)));

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
                    ReferenceTo(mech),
                    Mathf.RoundToInt(costModifier * simGame.Constants.Finances.MechCostPerQuarter)))
                .ToList(),
            simGame.PilotRoster
                .Select(pilot => new PilotExpense(
                    ReferenceTo(pilot.pilotDef.Description),
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

    // The commander is kept apart from the roster; the barracks lists them first (SGBarracksWidget.Reset).
    private static List<Pilot> ReadPilots(SimGameState simGame) =>
        new[] { simGame.Commander }
            .Concat(simGame.PilotRoster)
            .Select(pilot => ReadPilot(simGame, pilot))
            .ToList();

    private static Pilot ReadPilot(SimGameState simGame, BattleTech.Pilot pilot)
    {
        var definition = pilot.pilotDef;
        var description = definition.Description;
        return new Pilot(
            description.Id,
            FullName(description),
            description.Callsign,
            ReadPilotType(simGame, pilot),
            description.Age,
            description.Gender,
            simGame.GetPilotFullExpertise(pilot),
            new Skills(pilot.Gunnery, pilot.Piloting, pilot.Guts, pilot.Tactics),
            new Experience(pilot.UnspentXP, pilot.SpentXP),
            SimGameState.GetPrimaryPilotAbilities(definition).Select(ability => ReferenceTo(ability.Description))
                .ToList(),
            pilot.Health,
            pilot.Injuries,
            ReadPilotStatus(pilot),
            simGame.GetPilotTimeoutTimeRemaining(pilot),
            ReadSpirits(simGame, pilot),
            new ServiceRecord(
                definition.MissionsPiloted,
                definition.MechKills,
                definition.OtherKills,
                definition.MissionsEjected,
                definition.LifetimeInjuries,
                simGame.DaysPassed - definition.DateOfHire));
    }

    // Mirrors SimGameState.GetPilotTypeColor.
    private static PilotType ReadPilotType(SimGameState simGame, BattleTech.Pilot pilot) =>
        pilot == simGame.Commander ? PilotType.Commander
        : pilot.pilotDef.IsVanguard ? PilotType.Vanguard
        : pilot.pilotDef.IsRonin ? PilotType.Ronin
        : PilotType.Regular;

    // Mirrors SGBarracksDossierPanel.SetPilot: injuries take precedence over an event timeout.
    private static PilotStatus ReadPilotStatus(BattleTech.Pilot pilot) =>
        pilot.Injuries > 0 ? PilotStatus.Injured
        : pilot.pilotDef.TimeoutRemaining > 0 ? PilotStatus.Unavailable
        : PilotStatus.Ready;

    private static Spirits? ReadSpirits(SimGameState simGame, BattleTech.Pilot pilot) =>
        pilot switch
        {
            { HasHighMorale: true } => new Spirits(
                SpiritsLevel.High,
                simGame.GetTemporaryTagLength(pilot, BattleTech.Pilot.PILOTDEFTAG_HIGH_MORALE)),
            { HasLowMorale: true } => new Spirits(
                SpiritsLevel.Low,
                simGame.GetTemporaryTagLength(pilot, BattleTech.Pilot.PILOTDEFTAG_LOW_MORALE)),
            _ => null
        };

    // The limits are hardcoded in SimGameState.CanPilotTakeAbility.
    private static SkillRules ReadSkillRules(SimGameState simGame)
    {
        var progression = simGame.Constants.Progression;
        return new SkillRules(
            3,
            2,
            ReadSkill(simGame, SkillType.Gunnery, progression.GunneryDefaultTooltip),
            ReadSkill(simGame, SkillType.Piloting, progression.PilotingDefaultTooltip),
            ReadSkill(simGame, SkillType.Guts, progression.GutsDefaultTooltip),
            ReadSkill(simGame, SkillType.Tactics, progression.TacticsDefaultTooltip));
    }

    // SimGameState.AbilityTree is keyed by the skill's name and indexed by level - 1; the barracks prices the
    // pip for level n at GetLevelCost(n - 1). Level 1 is where every pilot starts, so the table begins at 2.
    private static Skill ReadSkill(SimGameState simGame, SkillType skill, string description)
    {
        var abilitiesByLevel = simGame.AbilityTree[skill.ToString()];
        return new Skill(
            description,
            Enumerable.Range(2, Math.Max(0, abilitiesByLevel.Count - 1))
                .Select(level => new SkillLevel(
                    level,
                    simGame.GetLevelCost(level - 1),
                    abilitiesByLevel[level - 1]
                        // The per-level accuracy traits have no name or description and aren't shown anywhere.
                        .Where(ability => !string.IsNullOrEmpty(ability.Description.Name))
                        .Select(ability => new SkillLevelAbility(
                            ReferenceTo(ability.Description),
                            ability.IsPrimaryAbility,
                            ability.Description.Details))
                        .ToList()))
                .ToList());
    }

    // Names, thresholds and resolve come from two constant files that mods can change separately;
    // only levels present in all three are complete.
    private static List<MoraleLevel> ReadMoraleLevels(SimGameState simGame)
    {
        var names = simGame.Constants.Story.MoraleLevelNames;
        var thresholds = simGame.CombatConstants.MoraleConstants.BaselineAddFromSimGameThresholds;
        var resolvePerTurn = simGame.CombatConstants.MoraleConstants.BaselineAddFromSimGameValues;
        return Enumerable.Range(0, Math.Min(names.Length, Math.Min(thresholds.Length, resolvePerTurn.Length)))
            .Select(level => new MoraleLevel(names[level], thresholds[level], resolvePerTurn[level]))
            .ToList();
    }

    // The game's level bounds are mixed (upper bounds below INDIFFERENT, lower ones above it), so each level's start
    // is found by classifying every reputation value SimGameState.ClampNewRepValue allows.
    private static List<ReputationLevel> ReadReputationLevels(SimGameState simGame)
    {
        var maxReputation = Mathf.RoundToInt(simGame.Constants.Story.MaxReputation);
        return Enumerable.Range(-maxReputation, 2 * maxReputation + 1)
            .GroupBy(value => simGame.GetReputation(value))
            .Select(level => new ReputationLevel(
                level.Key,
                level.First(),
                ReadMaxContractDifficulty(simGame, level.Key),
                // Mirrors StarSystem.CanUseSystemStore.
                level.Key > SimGameReputation.LOATHED,
                // Rounded as SG_Stores_MiniFactionWidget shows it.
                Mathf.RoundToInt(simGame.GetReputationShopAdjustment(level.Key) * 100f)))
            .ToList();
    }

    // Mirrors SimGameState.ContractUserMeetsReputation_Campaign, which compares the contract's whole-number
    // difficulty with this sum. The reputation tooltip (ReputationTooltipData) rounds it instead, so it can
    // show one more.
    private static int ReadMaxContractDifficulty(SimGameState simGame, SimGameReputation level)
    {
        var story = simGame.Constants.Story;
        return Mathf.FloorToInt(
            level switch
            {
                SimGameReputation.LOATHED => story.LoathedMaxContractDifficulty,
                SimGameReputation.HATED => story.HatedMaxContractDifficulty,
                SimGameReputation.DISLIKED => story.DislikedMaxContractDifficulty,
                SimGameReputation.INDIFFERENT => story.IndifferentMaxContractDifficulty,
                SimGameReputation.LIKED => story.LikedMaxContractDifficulty,
                SimGameReputation.FRIENDLY => story.FriendlyMaxContractDifficulty,
                _ => story.HonoredMaxContractDifficulty
            }
            + simGame.GlobalDifficulty);
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

    // TravelTime only counts the current leg (e.g. to the jump point). The travel order keeps the legs as
    // internal sub-entries, so its remaining cost is the whole trip, the single entry the queue shows.
    private static Travel? ReadTravel(SimGameState simGame)
    {
        var destination = simGame.Starmap?.Destination?.System;
        var travelOrder = simGame.TravelOrder;
        return simGame.TravelState == SimGameTravelStatus.IN_SYSTEM || destination is null || travelOrder is null
            ? null
            : new Travel(
                ReferenceTo(destination.Def.Description),
                ReferenceTo(destination.OwnerValue),
                travelOrder.GetRemainingCost());
    }

    private static DefinitionReference ReferenceTo(BaseDescriptionDef description) =>
        new(description.Id, description.Name);

    // A mech's name is its nickname (renameable in the mech lab).
    private static DefinitionReference ReferenceTo(MechDef mech) =>
        new(mech.Description.Id, MechReader.NameWithVariant(mech.Name, mech.Chassis));

    private static PilotReference ReferenceTo(HumanDescriptionDef pilot) =>
        new(pilot.Id, FullName(pilot), pilot.Callsign);

    private static string FullName(HumanDescriptionDef pilot) =>
        $"{pilot.FirstName} {pilot.LastName}".Trim() is { Length: > 0 } fullName ? fullName : pilot.Name;

    private static DefinitionReference ReferenceTo(FactionValue faction) =>
        new(faction.FactionDefID, FactionNames.Format(faction));
}
