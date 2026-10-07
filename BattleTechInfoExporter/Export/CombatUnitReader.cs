using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using BattleTechInfoExporter.Models;
using UnityEngine;
using Mech = BattleTech.Mech;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the combat state's units, each with as much as the player's HUD shows of it.</summary>
internal static class CombatUnitReader
{
    internal static SortedDictionary<string, CombatUnit> ReadUnits(CombatGameState combat)
    {
        var knownUnits = new List<(AbstractActor Actor, UnitAllegiance Allegiance, UnitVisibility Visibility)>();
        foreach (var actor in combat.AllActors)
        {
            var allegiance = ReadAllegiance(combat, actor);
            if (ReadVisibility(combat, actor, allegiance) is { } visibility)
            {
                knownUnits.Add((actor, allegiance, visibility));
            }
        }

        var playerUnits = knownUnits
            .Where(unit => unit.Allegiance == UnitAllegiance.Player && !unit.Actor.IsDead)
            .Select(unit => unit.Actor)
            .ToList();
        var targetableEnemies = knownUnits
            .Where(unit => unit.Allegiance == UnitAllegiance.Enemy && unit.Visibility == UnitVisibility.Full)
            .Select(unit => unit.Actor)
            .ToList();

        var units = new SortedDictionary<string, CombatUnit>(StringComparer.Ordinal);
        foreach (var (actor, allegiance, visibility) in knownUnits)
        {
            var targets = allegiance == UnitAllegiance.Player && !actor.IsDead ? targetableEnemies
                : allegiance == UnitAllegiance.Enemy && visibility == UnitVisibility.Full ? playerUnits
                : null;
            units.Add(actor.GUID, ReadUnit(combat, actor, allegiance, visibility, targets));
        }

        return units;
    }

    private static UnitAllegiance ReadAllegiance(CombatGameState combat, AbstractActor actor) =>
        actor.team == combat.LocalPlayerTeam
            ? UnitAllegiance.Player
            : combat.HostilityMatrix.GetHostilityOfLocalPlayer(actor.team) switch
            {
                Hostility.FRIENDLY => UnitAllegiance.Ally,
                Hostility.ENEMY => UnitAllegiance.Enemy,
                _ => UnitAllegiance.Neutral
            };

    // Mirrors the HUD (AbstractActor.OnPlayerVisibilityChanged): its own side and scripted reveals in full view, the
    // others as the player's side, allies included, sees them. Destroyed and never detected enemies are left out.
    private static UnitVisibility? ReadVisibility(
        CombatGameState combat,
        AbstractActor actor,
        UnitAllegiance allegiance)
    {
        if (allegiance is UnitAllegiance.Player or UnitAllegiance.Ally)
        {
            return UnitVisibility.Full;
        }

        if (actor.IsDead)
        {
            return null;
        }

        if (actor.IsForcedVisible)
        {
            return UnitVisibility.Full;
        }

        var playerTeam = combat.LocalPlayerTeam;
        return playerTeam.VisibilityToTarget(actor) switch
        {
            // In sight but hidden by ECM (BlipGhost), which hides only its pilot, heat, stability and initiative.
            VisibilityLevel.LOSFull or VisibilityLevel.BlipGhost => UnitVisibility.Full,
            VisibilityLevel.Blip4Maximum => UnitVisibility.BlipMaximum,
            VisibilityLevel.Blip1Type => UnitVisibility.BlipType,
            VisibilityLevel.None => playerTeam.VisibilityCache.previouslyDetectedEnemyLocations.ContainsKey(actor)
                ? UnitVisibility.LastSeen
                : null,
            // Blip0Minimum, and the blob levels the game never reaches.
            _ => UnitVisibility.BlipMinimum
        };
    }

    private static CombatUnit ReadUnit(
        CombatGameState combat,
        AbstractActor actor,
        UnitAllegiance allegiance,
        UnitVisibility visibility,
        IReadOnlyList<AbstractActor>? targets)
    {
        var isSighted = visibility == UnitVisibility.Full;
        var isLastSeen = visibility == UnitVisibility.LastSeen;
        var bayMech = allegiance == UnitAllegiance.Player && actor is Mech mech
            ? MechReader.ReferenceToBayMech(mech.MechDef)
            : null;
        return new CombatUnit(
            DefinitionReferences.ReferenceTo(actor.team.FactionValue),
            allegiance,
            visibility,
            isSighted || visibility is UnitVisibility.BlipMaximum or UnitVisibility.BlipType ? ReadKind(actor) : null,
            // What Mech, Vehicle and Turret.GetActorInfoFromVisLevel show at Blip4Maximum.
            visibility == UnitVisibility.BlipMaximum ? ReadTonnage(actor) : null,
            visibility == UnitVisibility.BlipMaximum && actor is Turret turret
                ? turret.TurretDef.Chassis.weightClass
                : null,
            isSighted && bayMech is null ? ReadDefinition(actor) : null,
            bayMech,
            ReadPosition(
                isLastSeen
                    ? combat.LocalPlayerTeam.VisibilityCache.previouslyDetectedEnemyLocations[actor]
                    : actor.CurrentPosition),
            isLastSeen ? null : actor.CurrentRotation.eulerAngles.y,
            isLastSeen ? null : actor.occupiedDesignMask?.Id,
            isSighted && actor.GetPilot() is { } pilot ? PilotReader.ReferenceTo(pilot) : null,
            isSighted ? ReadState(actor) : null,
            targets is null ? null : ReadLinesOfFire(actor, targets));
    }

    private static ArgumentException NoUnitKind(AbstractActor actor) =>
        new($"{actor.DisplayName} is no mech, vehicle or turret", nameof(actor));

    private static UnitKind ReadKind(AbstractActor actor) =>
        actor switch
        {
            Mech => UnitKind.Mech,
            Vehicle => UnitKind.Vehicle,
            Turret => UnitKind.Turret,
            _ => throw NoUnitKind(actor)
        };

    private static float? ReadTonnage(AbstractActor actor) =>
        actor switch
        {
            Mech mech => mech.MechDef.Chassis.Tonnage,
            Vehicle vehicle => vehicle.VehicleDef.Chassis.Tonnage,
            _ => null
        };

    // Named as in the catalog: a mech with its variant, a vehicle or turret as the HUD names it.
    private static DefinitionReference ReadDefinition(AbstractActor actor) =>
        actor switch
        {
            Mech mech => MechReader.ReferenceTo(mech.MechDef),
            Vehicle vehicle => DefinitionReferences.ReferenceTo(vehicle.VehicleDef.Description),
            Turret turret => DefinitionReferences.ReferenceTo(turret.TurretDef.Description),
            _ => throw NoUnitKind(actor)
        };

    internal static MapPosition ReadPosition(Vector3 position) => new(position.x, position.y, position.z);

    private static CombatUnitState ReadState(AbstractActor actor)
    {
        var mech = actor as Mech;
        var pilot = actor.GetPilot();
        return new CombatUnitState(
            actor.IsDead,
            ReadLocations(actor),
            actor.EvasivePipsCurrent,
            new Guard(actor.GuardLevel, actor.BracedLastRound, actor.HasCover, actor.HasBulwarkAbility),
            actor.IsEntrenched,
            actor.IsProne,
            actor.IsShutDown,
            actor.IsUnsteady,
            mech?.CurrentHeat,
            mech?.CurrentStability,
            actor.HasActivatedThisRound,
            actor.Initiative,
            actor.allComponents.Select(component => ReadComponent(actor, component)).ToList(),
            pilot?.Injuries,
            pilot?.Health,
            ReadAbilities(actor),
            actor.OffensivePushCost,
            actor.DefensivePushCost);
    }

    // Whole points, as the paper doll shows them (HUDMechArmorReadout.FormatForSummary, which the vehicle and turret
    // readouts share): cut off, except that a remainder below 1 shows as 1.
    private static UnitLocation ReadLocation(string location, float armor, float? rearArmor, float structure) =>
        new(
            location,
            HUDMechArmorReadout.FormatForSummary(armor),
            rearArmor is { } rear ? HUDMechArmorReadout.FormatForSummary(rear) : null,
            HUDMechArmorReadout.FormatForSummary(structure));

    private static List<UnitLocation> ReadLocations(AbstractActor actor) =>
        actor switch
        {
            Mech mech => MechReader.Locations
                .Select(location => ReadLocation(
                    location.ToString(),
                    // A location's front armor has the location's value (MechStructureRules.GetArmorFromChassisLocation).
                    mech.GetCurrentArmor((ArmorLocation)location),
                    RearArmorOf(location) is { } rearArmor ? mech.GetCurrentArmor(rearArmor) : null,
                    mech.GetCurrentStructure(location)))
                .ToList(),
            Vehicle vehicle => CatalogReader.VehicleLocationsOf(vehicle.VehicleDef.Chassis)
                .Select(location => ReadLocation(
                    location.ToString(),
                    vehicle.GetCurrentArmor(location),
                    null,
                    vehicle.GetCurrentStructure(location)))
                .ToList(),
            Turret turret =>
            [
                ReadLocation(
                    nameof(BuildingLocation.Structure),
                    turret.GetCurrentArmor(BuildingLocation.Structure),
                    null,
                    turret.GetCurrentStructure(BuildingLocation.Structure))
            ],
            _ => throw NoUnitKind(actor)
        };

    // MechStructureRules.GetChassisLocationFromArmorLocation in reverse, for the torso, which alone has rear armor.
    private static ArmorLocation? RearArmorOf(ChassisLocations location) =>
        location switch
        {
            ChassisLocations.LeftTorso => ArmorLocation.LeftTorsoRear,
            ChassisLocations.CenterTorso => ArmorLocation.CenterTorsoRear,
            ChassisLocations.RightTorso => ArmorLocation.RightTorsoRear,
            _ => null
        };

    // MechComponent.Location holds each kind of unit's own location enum; a turret's is BuildingLocation.Structure.
    private static string ReadLocationName(AbstractActor actor, int location) =>
        actor switch
        {
            Mech => ((ChassisLocations)location).ToString(),
            Vehicle => ((VehicleChassisLocations)location).ToString(),
            _ => ((BuildingLocation)location).ToString()
        };

    // AmmunitionBox.CurrentAmmo is 0 for a box that isn't functional. A weapon's own CurrentAmmo adds the boxes it
    // shares with other weapons, so only the ammo it carries itself counts here.
    private static CombatComponent ReadComponent(AbstractActor actor, MechComponent component) =>
        new(
            ComponentReferences.ReferenceTo(component.baseComponentRef),
            ReadLocationName(actor, component.Location),
            component.DamageLevel,
            component switch
            {
                AmmunitionBox box => box.CurrentAmmo,
                Weapon { AmmoCategoryValue.UsesInternalAmmo: true } weapon => weapon.InternalAmmo,
                _ => null
            });

    // The abilities a unit activates, as Pilot.InitAbilities sorts them into ActiveAbilities; a component's ability
    // works only while the component does (Ability.IsAvailable).
    private static List<CombatAbility> ReadAbilities(AbstractActor actor) =>
        (actor.GetPilot()?.ActiveAbilities ?? [])
        .Concat(actor.ComponentAbilities.Where(ability =>
            ability.Def.ActivationTime is not (AbilityDef.ActivationTiming.Passive
                or AbilityDef.ActivationTiming.NotSet)))
        .Select(ability => new CombatAbility(
            DefinitionReferences.ReferenceTo(ability.Def.Description),
            ability.CurrentCooldown,
            ability.Def.NumberOfUses > 0 ? ability.NumUsesLeft : null))
        .ToList();

    // Mirrors FiringPreviewManager.Recalc at the unit's own position, for a unit that can't turn first. The line of
    // fire is the one the unit's VisibilityCache keeps from where it stands, as ToHit.GetToHitChance reads it.
    private static List<LineOfFire> ReadLinesOfFire(AbstractActor attacker, IReadOnlyList<AbstractActor> targets)
    {
        var maxRange = attacker.GetLongestRangeWeapon(false)?.MaxRange ?? 0f;
        var maxIndirectRange = attacker.GetLongestRangeWeapon(false, true)?.MaxRange ?? 0f;
        return targets
            .Select(target =>
            {
                // A pair the cache hasn't computed yet holds VisibilityLevelAndAttribution.Unseen.
                var cachedLevel = attacker.VisibilityCache.VisibilityToTarget(target).LineOfFireLevel;
                var level = cachedLevel == LineOfFireLevel.NotSet
                    ? attacker.Combat.LOS.GetLineOfFire(attacker, target, out _)
                    : cachedLevel;
                var distance = Vector3.Distance(attacker.CurrentPosition, target.CurrentPosition);
                var fire = level > LineOfFireLevel.LOFBlocked
                    ? distance < maxRange ? FireAvailability.Direct : FireAvailability.OutOfRange
                    : distance < maxIndirectRange && !target.HasIndirectFireImmunity
                        ? FireAvailability.Indirect
                        : FireAvailability.None;
                return new LineOfFire(
                    target.GUID,
                    level switch
                    {
                        LineOfFireLevel.LOFClear => LineOfFireBlocking.Clear,
                        LineOfFireLevel.LOFObstructed => LineOfFireBlocking.PartiallyBlocked,
                        _ => LineOfFireBlocking.Blocked
                    },
                    fire,
                    attacker.IsInFiringArc(target, attacker.CurrentPosition, attacker.CurrentRotation));
            })
            .ToList();
    }
}
