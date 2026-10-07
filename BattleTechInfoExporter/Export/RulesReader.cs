using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using UnityEngine;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the rules file's tables from the game's rules for the career.</summary>
internal static class RulesReader
{
    internal static Rules Read(SimGameState simGame, ExportTrigger trigger) =>
        new(
            ModAssembly.Version,
            trigger,
            ReadMoraleLevels(simGame),
            ReadReputationLevels(simGame),
            ReadSkillRules(simGame),
            ReadSpiritsLevels(simGame),
            simGame.Constants.Story.DefaultMechPartMax,
            ContractReader.ReadContractTypes(simGame),
            ReadJumpDistances(simGame),
            CombatRulesReader.Read(simGame),
            CampaignRulesReader.Read(simGame));

    // Mech.JumpDistance indexes the move table by the number of working jump jets, clamped to its last entry, and
    // is 0 without any, so the first entry is never used.
    private static List<JumpDistance> ReadJumpDistances(SimGameState simGame) =>
        simGame.CombatConstants.MoveConstants.MoveTable
            .Skip(1)
            .Select((distance, index) => new JumpDistance(index + 1, distance))
            .ToList();

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
        var toHit = simGame.CombatConstants.ToHit;
        var piloting = simGame.CombatConstants.PilotingConstants;
        return new Skill(
            GameText.ToPlainText(description),
            Enumerable.Range(2, Math.Max(0, abilitiesByLevel.Count - 1))
                .Select(level => new SkillLevel(
                    level,
                    simGame.GetLevelCost(level - 1),
                    // Mirrors ToHit.GetBaseToHitChance.
                    skill == SkillType.Gunnery
                        ? HitChancePercent(toHit.ToHitBaseFloor, level, toHit.ToHitGunneryDivisor)
                        : null,
                    // Mirrors ToHit.GetBaseMeleeToHitChance.
                    skill == SkillType.Piloting
                        ? HitChancePercent(piloting.PilotingBaseFloor, level, piloting.PilotingDivisor)
                        : null,
                    // Mirrors Pathing.ResetPathGrid after standing up, whose 0.75 is hardcoded.
                    skill == SkillType.Piloting
                        ? RoundedPercent(1f - (0.75f - level / piloting.PilotingDivisor))
                        : null,
                    abilitiesByLevel[level - 1]
                        // The per-level accuracy traits have no name and no effects, only a label for the
                        // level's hit chance.
                        .Where(ability => !string.IsNullOrEmpty(ability.Description.Name))
                        .Select(ability => AbilityReader.Read(simGame, ability))
                        .ToList()))
                .ToList());
    }

    private static float HitChancePercent(float floor, int level, float divisor) =>
        RoundedPercent(floor + level / divisor);

    // Rounded to a tenth, so float noise doesn't show.
    private static float RoundedPercent(float share) => Mathf.Round(share * 1000f) / 10f;

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

    // Mirrors AbstractActor.OffensivePushCost (Precision Strike) and DefensivePushCost (Vigilance). Combat reads
    // them from CombatGameConstants.GetActiveMoraleDef, which only returns FuryConstants in Arena Skirmish.
    private static List<SpiritsLevelCosts> ReadSpiritsLevels(SimGameState simGame)
    {
        var morale = simGame.CombatConstants.MoraleConstants;
        return
        [
            new SpiritsLevelCosts(SpiritsLevel.Normal, morale.OffensivePushCost, morale.DefensivePushCost),
            new SpiritsLevelCosts(
                SpiritsLevel.High,
                morale.OffensivePushHighMoraleCost,
                morale.DefensivePushHighMoraleCost),
            new SpiritsLevelCosts(
                SpiritsLevel.Low,
                morale.OffensivePushLowMoraleCost,
                morale.DefensivePushLowMoraleCost)
        ];
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
}
