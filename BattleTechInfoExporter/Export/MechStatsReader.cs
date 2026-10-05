using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using UnityEngine;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds a mech's performance summary: the ratings come from <see cref="MechStatisticsRules" />, the numbers
///     mirror the stat tooltips (<see cref="StatTooltipData" />), which only return them as display text.
/// </summary>
internal static class MechStatsReader
{
    // COIL weapons gain damage with the mech's evasion; the stats assume this many evasion pips
    // (StatTooltipData.COIL_EVASIVEPIP_AVG).
    private const int CoilEvasionPips = 3;

    internal static MechStats Read(MechDef mech)
    {
        // The tooltips count only components that work or are being installed.
        var workingComponents = mech.Inventory
            .Where(component => component.DamageLevel is ComponentDamageLevel.Functional
                or ComponentDamageLevel.Installing)
            .Select(component => component.Def)
            .ToList();
        var weapons = workingComponents.OfType<WeaponDef>().ToList();
        var jumpJets = workingComponents.OfType<JumpJetDef>().ToList();
        var upgrades = workingComponents.OfType<UpgradeDef>().ToList();
        return new MechStats(
            new StatRatings(
                Rate(mech, MechStatisticsRules.CalculateFirepowerStat),
                Rate(mech, MechStatisticsRules.CalculateMovementStat),
                Rate(mech, MechStatisticsRules.CalculateRangeStat),
                Rate(mech, MechStatisticsRules.CalculateHeatEfficiencyStat),
                Rate(mech, MechStatisticsRules.CalculateDurabilityStat),
                Rate(mech, MechStatisticsRules.CalculateMeleeStat)),
            ReadMovement(mech, jumpJets),
            ReadHeat(mech, weapons, jumpJets, workingComponents.OfType<HeatSinkDef>().ToList()),
            ReadFirepower(weapons),
            ReadRange(weapons),
            ReadDurability(mech, upgrades),
            ReadMelee(mech, upgrades));
    }

    // StatTooltipData.SetData shows negative ratings as zero.
    private static int Rate(MechDef mech, StatCalculation calculate)
    {
        float rating = 0, maxRating = 0;
        calculate(mech, ref rating, ref maxRating);
        return Mathf.RoundToInt(Math.Max(rating, 0));
    }

    // Mirrors StatTooltipData.SetMovementData. The jump distance is clamped to the move table as in
    // Mech.JumpDistance; the tooltip itself fails on a mech with as many jump jets as the table has entries.
    private static MovementStats ReadMovement(MechDef mech, List<JumpJetDef> jumpJets)
    {
        var movement = mech.Chassis.MovementCapDef;
        var moveTable = MechStatisticsRules.Combat.MoveConstants.MoveTable;
        var jumpDistance = jumpJets.Count == 0
            ? 0
            : moveTable[Math.Min(jumpJets.Count, moveTable.Length - 1)]
              * (1 + SumEffects(
                  jumpJets,
                  "JumpDistanceMultiplier",
                  StatisticEffectData.TargetCollection.NotSet,
                  StatCollection.StatOperation.Float_Add,
                  WeaponSubType.NotSet));
        return new MovementStats(movement.MaxWalkDistance, movement.MaxSprintDistance, jumpDistance);
    }

    // Mirrors StatTooltipData.SetHeatData, but takes the shutdown heat from the combat constants, as
    // MechStatisticsRules.CalculateHeatEfficiencyStat does, instead of the tooltip's literal 100.
    private static HeatStats ReadHeat(
        MechDef mech,
        List<WeaponDef> weapons,
        List<JumpJetDef> jumpJets,
        List<HeatSinkDef> heatSinks)
    {
        var heat = MechStatisticsRules.Combat.Heat;
        var weaponHeat = weapons.Sum(weapon =>
            weapon.Type == WeaponType.COIL ? weapon.Damage * CoilEvasionPips / 2 : weapon.HeatGenerated);
        var alphaStrikeHeat = heatSinks
            .SelectMany(heatSink => heatSink.statusEffects)
            .Select(effect => EffectModifier(
                effect,
                "HeatGenerated",
                StatisticEffectData.TargetCollection.Weapon,
                StatCollection.StatOperation.Float_Multiply,
                WeaponSubType.NotSet))
            .Where(multiplier => multiplier > 0)
            .Aggregate(weaponHeat, (total, multiplier) => total * multiplier);
        return new HeatStats(
            mech.Chassis.Heatsinks
            + heat.InternalHeatSinkCount * heat.DefaultHeatSinkDissipationCapacity
            + heatSinks.Sum(heatSink => heatSink.DissipationCapacity),
            alphaStrikeHeat,
            jumpJets.Count * heat.JumpHeatPerUnit * heat.JumpHeatUnitSize / 3,
            heat.MaxHeat + SumEffects(
                heatSinks,
                "MaxHeat",
                StatisticEffectData.TargetCollection.NotSet,
                StatCollection.StatOperation.Int_Add,
                WeaponSubType.NotSet));
    }

    // Mirrors MechStatisticsRules.CalculateFirepowerStat, the rating's source; the tooltip doesn't multiply the
    // stability damage of most weapons by their shots.
    private static FirepowerStats ReadFirepower(List<WeaponDef> weapons) =>
        new(
            weapons.Sum(weapon =>
                weapon.Damage * (weapon.Type == WeaponType.COIL ? CoilEvasionPips : 1) * weapon.ShotsWhenFired),
            weapons.Sum(weapon => weapon.Instability * weapon.ShotsWhenFired));

    // Mirrors StatTooltipData.SetRangeData.
    private static RangeStats ReadRange(List<WeaponDef> weapons) =>
        weapons.Count == 0
            ? new RangeStats(0, 0)
            : new RangeStats(weapons.Max(weapon => weapon.MaxRange), weapons.Average(weapon => weapon.MediumRange));

    /// <summary>The chassis' melee values, which upgrades add to, as the stat tooltips show them.</summary>
    // StatTooltipData.SetMeleeData doubles the jump attack's damage.
    internal static ChassisMelee ReadChassisMelee(ChassisDef chassis) =>
        new(chassis.MeleeDamage, chassis.MeleeInstability, chassis.DFADamage * 2, chassis.DFASelfDamage);

    // Mirrors StatTooltipData.SetDurabilityData, except for its stability defense.
    private static DurabilityStats ReadDurability(MechDef mech, List<UpgradeDef> upgrades) =>
        new(
            mech.MechDefCurrentArmor,
            mech.MechDefCurrentStructure,
            mech.Chassis.DFASelfDamage - SumEffects(
                upgrades,
                "DFASelfDamage",
                StatisticEffectData.TargetCollection.NotSet,
                StatCollection.StatOperation.Float_Subtract,
                WeaponSubType.DFA));

    // Mirrors StatTooltipData.SetMeleeData.
    private static MeleeStats ReadMelee(MechDef mech, List<UpgradeDef> upgrades)
    {
        var chassisMelee = ReadChassisMelee(mech.Chassis);
        return new MeleeStats(
            chassisMelee.Damage + SumWeaponBonuses("DamagePerShot", WeaponSubType.Melee),
            chassisMelee.StabilityDamage + SumWeaponBonuses("Instability", WeaponSubType.Melee),
            chassisMelee.DeathFromAboveDamage + SumWeaponBonuses("DamagePerShot", WeaponSubType.DFA));

        float SumWeaponBonuses(string statName, WeaponSubType attack) =>
            SumEffects(
                upgrades,
                statName,
                StatisticEffectData.TargetCollection.Weapon,
                StatCollection.StatOperation.Float_Add,
                attack);
    }

    private static float SumEffects(
        IEnumerable<MechComponentDef> components,
        string statName,
        StatisticEffectData.TargetCollection target,
        StatCollection.StatOperation operation,
        WeaponSubType weaponSubType) =>
        components
            .SelectMany(component => component.statusEffects)
            .Sum(effect => EffectModifier(effect, statName, target, operation, weaponSubType));

    // Mirrors StatTooltipData.GetEffectMod: the effect's value if it changes the given stat in the given way.
    private static float EffectModifier(
        EffectData effect,
        string statName,
        StatisticEffectData.TargetCollection target,
        StatCollection.StatOperation operation,
        WeaponSubType weaponSubType) =>
        effect.statisticData is { } statistic
        && statistic.statName == statName
        && statistic.targetCollection == target
        && statistic.operation == operation
        && statistic.targetWeaponSubType == weaponSubType
            ? float.Parse(statistic.modValue, CultureInfo.InvariantCulture)
            : 0;

    private delegate void StatCalculation(MechDef mech, ref float rating, ref float maxRating);
}
