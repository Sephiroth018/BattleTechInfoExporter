using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using UnityEngine;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds the rules file's combat rules from the combat constants (CombatGameConstants). Only constants that
///     the game's combat code reads are exported; the ones it never reads, reads only in code nothing calls, or that
///     only drive presentation or the AI are left out.
/// </summary>
internal static class CombatRulesReader
{
    // ToHit.GetUMChance rounds the chance to the nearest 5% and clamps it.
    private const int RoundedToPercent = 5;
    private const int MinChancePercent = 5;
    private const int MaxChancePercent = 95;

    internal static CombatRules Read(SimGameState simGame)
    {
        var constants = simGame.CombatConstants;
        return new CombatRules(
            ReadToHit(constants),
            ReadEvasion(constants),
            ReadGuard(constants.ResolutionConstants),
            ReadLineOfFire(constants),
            ReadHeat(constants),
            ReadStability(constants),
            new InjuryRules(
                constants.PilotingConstants.InjuryFromSideTorsoDestruction,
                constants.PilotingConstants.InjuryFromAmmoExplosion,
                constants.Heat.ShutdownCausesInjury),
            // ToHit.GetDFAModifier and Pathing's melee destinations.
            new MeleeAttackRules(
                constants.PilotingConstants.PilotingDFAModifier,
                constants.MoveConstants.MaxMeleeVerticalOffset),
            ReadCriticalHits(constants.ResolutionConstants),
            ReadHitTables(constants),
            ReadVisibility(constants),
            // Combat reads the morale constants from CombatGameConstants.GetActiveMoraleDef, which only returns
            // FuryConstants in Arena Skirmish.
            ReadResolve(constants.MoraleConstants),
            ReadMovement(constants.MoveConstants));
    }

    // The modifiers are summed in ToHit.GetAllModifiers and turned into the chance in ToHit.GetUMChance; the
    // stepped algorithm (ToHit.GetSteppedValue) is the one the game's data switches on.
    private static ToHitRules ReadToHit(CombatGameConstants constants)
    {
        var toHit = constants.ToHit;
        return new ToHitRules(
            toHit.ToHitStepThresholds
                .Select((threshold, index) => new DifficultyStep(threshold, toHit.ToHitStepValues[index]))
                .ToList(),
            RoundedToPercent,
            MinChancePercent,
            MaxChancePercent,
            constants.ResolutionConstants.AllowTotalNegativeModifier,
            // ToHit.GetRangeModifierForDist.
            new RangeBandModifiers(
                toHit.ToHitMinimumRange,
                toHit.ToHitShortRange,
                toHit.ToHitMediumRange,
                toHit.ToHitLongRange,
                toHit.ToHitMaximumRange),
            // ToHit.GetTargetSizeModifier.
            new TargetWeightClassModifiers(
                new ByWeightClass<float>(toHit.ToHitLight, toHit.ToHitMedium, toHit.ToHitHeavy, toHit.ToHitAssault),
                new ByWeightClass<float>(
                    toHit.ToHitVehicleLight,
                    toHit.ToHitVehicleMedium,
                    toHit.ToHitVehicleHeavy,
                    toHit.ToHitVehicleAssault),
                new ByWeightClass<float>(
                    toHit.ToHitTurretLight,
                    toHit.ToHitTurretMedium,
                    toHit.ToHitTurretHeavy,
                    toHit.ToHitTurretAssault)),
            // ToHit.GetHeightDelta and GetHeightModifier.
            new ElevationModifier(
                toHit.ToHitElevationLevelHeight,
                toHit.ToHitElevationModifierPerLevel,
                toHit.ToHitElevationUseMultipleLevels,
                toHit.ToHitElevationApplyPenalties),
            toHit.ToHitShutdownTargetBonus,
            toHit.ToHitProneTargetBonus,
            toHit.ToHitSelfSprinted,
            // ToHit.GetSelfSpeedModifier.
            new AttackerWalkedModifiers(
                new ByWeightClass<float>(
                    toHit.ToHitSelfWalkLight,
                    toHit.ToHitSelfWalkMedium,
                    toHit.ToHitSelfWalkHeavy,
                    toHit.ToHitSelfWalkAssault),
                toHit.ToHitSelfWalkVehicle),
            toHit.ToHitSelfStoodUp,
            toHit.ToHitSelfOverheated,
            toHit.ToHitSelfArmMountedWeapon,
            // MechStructureRules.GetToHitModifier.
            toHit.ToHitSelfWeaponDamaged,
            new DamagedLocationModifiers(
                toHit.ToHitSelfWeaponLocationDamagedMinor,
                toHit.ToHitSelfWeaponLocationDamagedMajor),
            toHit.ToHitOffensivePush,
            // Mech.IsTargetPositionInFiringArc; a turret has its own (TurretChassisDef.FiringArcDegrees).
            toHit.FiringArcDegrees,
            // Turret.SkillGunnery.
            constants.Skills.TurretDefaultGunnery);
    }

    // Pips from Mech.GetEvasivePipsResult and Vehicle.GetEvasivePipsResult; their modifier from
    // ToHit.GetEvasivePipsModifier, which weapons the pips apply to from ToHit.WeaponIsAffectedByEvasive.
    private static EvasionRules ReadEvasion(CombatGameConstants constants)
    {
        var toHit = constants.ToHit;
        var resolution = constants.ResolutionConstants;
        return new EvasionRules(
            toHit.ToHitMovingTargetDistances
                .Select((distance, index) => new EvasivePips(distance, toHit.EvasivePipsMovingTarget[index]))
                .ToList(),
            toHit.ToHitMovingPipUMs,
            toHit.WeaponsAffectedByEvasive.Select(weaponType => (WeaponType)weaponType).ToList(),
            resolution.VehicleEvasiveResultMultiplier,
            resolution.VehiclesGetEvasive);
    }

    // The levels are ToHit.GetBlowQuality's: solid without guard, then normal, glancing and ineffective; the
    // multipliers ToHit.GetBlowQualityMultiplier's. What breaks the guard is in AbstractActor.GuardLevel.
    private static GuardRules ReadGuard(CombatResolutionConstantsDef resolution) =>
        new(
            [
                resolution.SolidBlowDamageMultiplier,
                resolution.NormalBlowDamageMultiplier,
                resolution.GlancingBlowDamageMultiplier,
                resolution.IneffectiveBlowDamageMultiplier
            ],
            resolution.UnsteadyCountersGuarded,
            resolution.SolidDamageCountersGuarded,
            resolution.SensorLockCountersGuarded,
            resolution.VehiclesGetGuarded);

    // The ratios are LineOfSight.GetLineOfFireUncached's, the modifiers ToHit.GetCoverModifier's and
    // GetIndirectModifier's, the damage multipliers AbstractActor.GetReducedDamage's.
    private static LineOfFireRules ReadLineOfFire(CombatGameConstants constants) =>
        new(
            constants.ToHit.ToHitCoverObstructed,
            constants.ToHit.DamageResistanceObstructed,
            constants.ToHit.ToHitIndirect,
            constants.ToHit.DamageResistanceIndirectFire,
            constants.Visibility.RatioFullVis,
            constants.Visibility.RatioObstructedVis,
            constants.Visibility.MinRatioFromActors);

    private static HeatRules ReadHeat(CombatGameConstants constants)
    {
        var heat = constants.Heat;
        return new HeatRules(
            heat.MaxHeat,
            // As Mech.InitStats sets the OverheatLevel statistic.
            (int)(heat.MaxHeat * heat.OverheatLevel),
            heat.WalkHeat,
            heat.SprintHeat,
            // Mech.CalcJumpHeat.
            new JumpHeat(heat.JumpHeatUnitSize, heat.JumpHeatPerUnit, heat.JumpHeatMin),
            heat.EngineDamageHeat,
            // Mech.GetHeatSinkDissipation.
            heat.InternalHeatSinkCount,
            heat.DefaultHeatSinkDissipationCapacity,
            // As Mech.ApplyStartupHeatSinks rounds it.
            (int)(heat.MaxHeat * (1f - heat.StartupSinkRatio)),
            heat.GlobalHeatIncreaseMultiplier,
            heat.GlobalHeatSinkMultiplier,
            // Mech.DamageFromOverheat.
            new ByWeightClass<float>(
                heat.CriticalHeatPerLocationLight,
                heat.CriticalHeatPerLocationMedium,
                heat.CriticalHeatPerLocationHeavy,
                heat.CriticalHeatPerLocationAssault));
    }

    // The recovery is Mech.GetMinStability's: the level the mech is in always empties, then the action's
    // levels. A negative dump adds levels instead (MechDFASequence, MechDisplacementSequence).
    private static StabilityRules ReadStability(CombatGameConstants constants)
    {
        var resolution = constants.ResolutionConstants;
        var piloting = constants.PilotingConstants;
        return new StabilityRules(
            Mathf.RoundToInt(resolution.DefaultUnsteadyThreshold),
            resolution.StabilityLevels,
            new LevelsRecoveredByAction(
                resolution.StabilityDumpStationary,
                resolution.StabilityDumpMoving,
                resolution.StabilityDumpJumping,
                resolution.StabilityDumpStoodUp,
                resolution.StabilityDumpBracing,
                resolution.StabilityDumpAbilityDefer),
            new LevelsAddedByAction(-resolution.StabilityDumpDFA, -resolution.StabilityDumpFalling),
            resolution.EntrenchedMultiplier,
            // Mech.UpdateMinStability; the game's data limits the loss to legs.
            piloting.OnlyPermanentLossFromLegs ? piloting.LocationDestroyedPermanentStabilityLoss : 0f,
            // Mech.ApplySideTorsoStructureEffects and the other Apply…StructureEffects methods.
            new InstabilityFromDamage(
                piloting.SideTorsoBlackRelativeInstability,
                piloting.CenterTorsoYellowRelativeInstability,
                piloting.ArmBlackRelativeInstability,
                piloting.LegDamageRelativeInstability));
    }

    // CritChanceRules.GetCritChance and GetCritMultiplier; the slot handling is Mech.GetComponentInSlot's.
    private static CriticalHitRules ReadCriticalHits(CombatResolutionConstantsDef resolution) =>
        new(resolution.MinCritChance, resolution.AICritChanceBaseMultiplier, resolution.SearchForValidCritSlot);

    // The direction's table is HitLocation.GetMechHitTable's and GetVehicleHitTable's; no attack ever comes from
    // the top, so those tables are left out. The called shot is AttackDirector.GetIndividualHits', the cluster
    // weights HitLocation.GetClusterTable's.
    private static HitTables ReadHitTables(CombatGameConstants constants)
    {
        var hitTables = constants.HitTables;
        var toHit = constants.ToHit;
        return new HitTables(
            new HitTablesOf<ArmorLocation>(
                hitTables.HitMechLocationFromFront,
                hitTables.HitMechLocationFromLeft,
                hitTables.HitMechLocationFromRight,
                hitTables.HitMechLocationFromBack,
                hitTables.HitMechLocationProne,
                // DamageOrderUtility.ApplyDamageToAllLocations skips the head.
                hitTables.HitMechLocationFromArtillery.Keys.Where(location => location != ArmorLocation.Head).ToList()),
            new HitTablesOf<VehicleChassisLocations>(
                hitTables.HitVehicleLocationFromFront,
                hitTables.HitVehicleLocationFromLeft,
                hitTables.HitVehicleLocationFromRight,
                hitTables.HitVehicleLocationFromBack,
                null,
                hitTables.HitVehicleLocationFromArtillery.Keys.ToList()),
            new CalledShotRules(hitTables.CalledShotBonusMultiplier, hitTables.CalledShotBonusDegradeLerpFactor),
            new ClusteredHitWeights(
                toHit.ClusterChanceOriginalLocationMultiplier,
                toHit.ClusterChanceAdjacentMultiplier,
                toHit.ClusterChanceNonadjacentMultiplier,
                !toHit.ClusterChanceNeverMultiplyHead));
    }

    // The ranges are LineOfSight.GetSpotterRange's and GetSensorRange's, the sensor lock SensorLockSequence's.
    private static VisibilityRules ReadVisibility(CombatGameConstants constants)
    {
        var visibility = constants.Visibility;
        var toHit = constants.ToHit;
        return new VisibilityRules(
            visibility.BaseSpotterDistance,
            visibility.SpotterTacticsMultiplier,
            visibility.BaseSensorDistance,
            visibility.SensorHysteresisAdditive,
            visibility.ShutdownSpottingDistanceMultiplier,
            visibility.ProneSpottingDistanceMultiplier,
            visibility.ShutDownSignatureModifier,
            visibility.ShutDownVisibilityModifier,
            visibility.GhostStateHidesBlips,
            new SensorLockRules(
                toHit.SensorLockStripsEvasivePips ? toHit.SensorLockPipsStripped : 0,
                EffectReader.ReadStatisticChanges(
                    Enumerable.Repeat(visibility.SensorsImpairedEffect, visibility.NumSensorLockImpairedEffects)
                        .Append(visibility.SensorLockAntiStealthEffect))),
            // AttackStackSequence applies both when a unit fires.
            EffectReader.ReadStatisticChanges([
                visibility.FiredWeaponsEffect, visibility.FiredWeaponsAntiStealthEffect
            ]));
    }

    // The gain per round is Team.ApplyBaselineMoraleGain's, the events AttackDirector.ResolveSequenceMorale's and
    // Team.OnObjectiveSucceeded's, the inspiration Mech.InspireActor's.
    private static ResolveRules ReadResolve(MoraleConstantsDef morale) =>
        new(
            morale.MoraleMin,
            morale.MoraleMax,
            morale.MoraleInitialDefault,
            morale.CanUseInspireLevel,
            morale.InspireCost,
            morale.AutoInspire,
            EffectReader.ReadStatisticChanges(morale.InspiredEffect),
            morale.MoraleBaselineGainPerRound,
            // Team.CollectUnitBaseline.
            morale switch
            {
                { MoraleBaselineMultiplyByActive: true } => ResolveGainMultiplier.LivingUnits,
                { BaselineMultiplyByActiveElites: true } => ResolveGainMultiplier.LivingElitePilots,
                _ => ResolveGainMultiplier.None
            },
            morale.BaselineAddFromSimGame,
            morale.BaselineAddMoraleMods,
            morale.BaselineModsOnlyBiggest,
            new ResolveEventThresholds(
                morale.ThresholdMajorityHit,
                morale.ThresholdMajorityMiss,
                morale.ThresholdMinorArmor,
                morale.ThresholdMajorArmor),
            new ResolveFromEvents(
                new ByWeightClass<int>(
                    morale.ChangeEnemyDestroyedLight,
                    morale.ChangeEnemyDestroyedMedium,
                    morale.ChangeEnemyDestroyedHeavy,
                    morale.ChangeEnemyDestroyedAssault),
                morale.ChangeEnemyDestroyedMeleeAdditional,
                morale.ChangeEnemyCrit,
                morale.ChangeEnemyAmmoExplodes,
                morale.ChangeEnemyWeaponDestroyed,
                morale.ChangeEnemyLocationDestroyed,
                morale.ChangeEnemyKnockedDown,
                morale.ChangeDFADealt,
                morale.ChangeEnemyMinorArmorDamage,
                morale.ChangeEnemyMajorArmorDamage,
                morale.ChangeMajorityAttackingShotsHit,
                morale.ChangeMajorityAttackingShotsMiss,
                morale.ChangeObjectiveCompleted,
                morale.ChangeObjectiveFailed),
            EffectReader.ReadStatisticChanges(morale.InitiativeDelayEffect),
            morale.CanAIBeInspired);

    // Mech.MoveMultiplier; the pivot is PathNodeGrid.BuildPathFromEnd's, the hex width HexGrid's.
    private static MovementRules ReadMovement(MovementConstants movement) =>
        new(
            movement.OverheatedMovePenalty,
            new LegDamagePenalties(
                movement.LegDamageYellowPenalty,
                movement.LegDamageRedPenalty,
                movement.LegDestroyedPenalty),
            movement.MinMoveSpeed,
            movement.RotateInPlaceThreshold,
            movement.ExperimentalGridDistance);
}
