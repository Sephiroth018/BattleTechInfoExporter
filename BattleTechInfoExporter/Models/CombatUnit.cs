using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A unit in combat, with as much of it as the HUD shows at its <see cref="Visibility" />.</summary>
/// <param name="Side">Whose side the unit is on.</param>
/// <param name="Visibility">How much of the unit the HUD shows, deciding which of its other fields are set.</param>
/// <param name="Kind">
///     The kind of unit; <c>null</c> when the HUD doesn't tell: at <see cref="UnitVisibility.BlipMinimum" /> and
///     <see cref="UnitVisibility.LastSeen" />.
/// </param>
/// <param name="Tonnage">
///     The unit's tonnage, in tons; <c>null</c> when the HUD doesn't tell: for a blip below
///     <see cref="UnitVisibility.BlipMaximum" />, a turret blip, which shows its weight class instead, and when
///     last seen.
/// </param>
/// <param name="WeightClass">
///     The unit's weight class; <c>null</c> when the HUD doesn't tell: for a blip below
///     <see cref="UnitVisibility.BlipMaximum" />, a mech or vehicle blip, which shows its tonnage instead, and when
///     last seen.
/// </param>
/// <param name="Definition">
///     The mech, vehicle or turret in the catalog, which holds its armor and loadout; <c>null</c> when the unit is
///     one of the player's mechs, which have <see cref="BayMech" /> instead, or out of full view.
/// </param>
/// <param name="BayMech">
///     The player's mech in <c>game-state.json</c>, whose loadout holds its assigned armor; <c>null</c> when the
///     unit isn't one.
/// </param>
/// <param name="Position">Where the unit is, or was last detected at for <see cref="UnitVisibility.LastSeen" />.</param>
/// <param name="FacingDegrees">
///     The direction the unit faces, in degrees clockwise from the map's +Z axis, from 0 to below 360 and to 0.1°,
///     which a blip's marker turns to at every level; <c>null</c> when last seen.
/// </param>
/// <param name="Terrain">
///     The terrain the unit stands in, keyed as in <see cref="Catalog.TerrainDefinitions" />, which a blip's
///     position on the map shows; <c>null</c> when on open ground or last seen.
/// </param>
/// <param name="Pilot">The pilot, which the HUD names only at <see cref="UnitVisibility.Full" />.</param>
/// <param name="State">The unit's condition, shown at <see cref="UnitVisibility.Full" />.</param>
/// <param name="LinesOfFire">
///     The lines of fire to the units it could attack: from the player's units to every living enemy at
///     <see cref="UnitVisibility.Full" />, and from those enemies to the player's units. <c>null</c> when the unit is
///     neither, or destroyed.
/// </param>
/// <param name="Movement">
///     Where the player's unit can move and what it gets there; <c>null</c> when the unit isn't the player's or can't
///     move (a turret, a prone or shut down mech).
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatUnit(
    TeamSide Side,
    UnitVisibility Visibility,
    UnitKind? Kind,
    float? Tonnage,
    WeightClass? WeightClass,
    DefinitionReference? Definition,
    MechReference? BayMech,
    MapPosition Position,
    float? FacingDegrees,
    string? Terrain,
    PilotReference? Pilot,
    CombatUnitState? State,
    IReadOnlyList<LineOfFire>? LinesOfFire,
    UnitMovement? Movement);

/// <summary>A unit's condition as the HUD shows it, for a unit in full view.</summary>
/// <param name="DeathMethod">
///     How the unit was destroyed (the game's <see cref="BattleTech.DeathMethod" />), e.g.
///     <c>LegDestruction</c>, or <c>PilotKilled</c> also for an incapacitated pilot; <c>null</c> when the unit lives.
/// </param>
/// <param name="Locations">
///     The armor and structure left per location, in the game's order: a mech's from head to legs, a vehicle's from
///     front to rear and turret, a turret's single one. The limits are in <see cref="CombatUnit.Definition" /> or
///     <see cref="CombatUnit.BayMech" />.
/// </param>
/// <param name="EvasionPips">The evasion pips the unit has, earned by moving (see <see cref="EvasionRules" />).</param>
/// <param name="Guard">The unit's guard level and its sources.</param>
/// <param name="IsEntrenched">
///     Whether the unit is entrenched, e.g. by bracing, until its next activation: weapons' stability damage against it
///     is multiplied by <see cref="StabilityRules.EntrenchedInstabilityMultiplier" />.
/// </param>
/// <param name="IsProne">Whether the mech has been knocked down; only mechs can be.</param>
/// <param name="IsShutDown">Whether the unit is shut down, e.g. from overheating.</param>
/// <param name="IsUnsteady">
///     Whether the mech is unsteady: its instability rose above <see cref="MechStability.UnsteadyAbove" /> and
///     hasn't fallen below since; only mechs can be.
/// </param>
/// <param name="Mech">What only a mech has; <c>null</c> when the unit is a vehicle or turret.</param>
/// <param name="HasActivated">Whether the unit has finished its activation this round.</param>
/// <param name="Initiative">The phase the unit acts in, on the scale of <see cref="CombatState.Phase" />.</param>
/// <param name="Components">Every component mounted, in the game's order.</param>
/// <param name="Pilot">The pilot's condition; <c>null</c> when the unit has no pilot.</param>
/// <param name="Abilities">The pilot's and components' abilities that can be activated, with their cooldowns.</param>
/// <param name="ResolveCosts">
///     What Precision Strike and Vigilance cost the unit now, which its pilot's spirits change; <c>null</c> when the
///     unit isn't in the player's lance, which alone spends resolve.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatUnitState(
    DeathMethod? DeathMethod,
    IReadOnlyList<UnitLocation> Locations,
    int EvasionPips,
    Guard Guard,
    bool IsEntrenched,
    bool IsProne,
    bool IsShutDown,
    bool IsUnsteady,
    MechState? Mech,
    bool HasActivated,
    int Initiative,
    IReadOnlyList<CombatComponent> Components,
    CombatPilotState? Pilot,
    IReadOnlyList<CombatAbility> Abilities,
    ResolveCosts? ResolveCosts);

/// <summary>What only a mech's condition has: its heat and stability bars.</summary>
/// <param name="Heat">The mech's heat.</param>
/// <param name="Stability">The mech's stability bar.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechState(MechHeat Heat, MechStability Stability);

/// <summary>A mech's stability as the HUD's stability bar shows it, with the thresholds it draws.</summary>
/// <param name="Current">The instability taken, from 0 up, as the bar fills.</param>
/// <param name="UnsteadyAbove">
///     The instability above which the mech is unsteady and loses its evasion pips, after effects; the base is
///     <see cref="StabilityRules.UnsteadyAbovePercent" /> of <see cref="MaxStability" />.
/// </param>
/// <param name="MaxStability">
///     The instability at which the mech is knocked down: the chassis' stability, as its <c>MaxStability</c>
///     statistic has it now.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechStability(float Current, float UnsteadyAbove, float MaxStability);

/// <summary>A mech's heat as the HUD's heat bar shows it, with the thresholds it draws and the sinking it predicts.</summary>
/// <param name="Current">The heat now.</param>
/// <param name="Dissipation">
///     The heat the mech sheds at the end of its activation now: its heat sinks after the terrain it stands in, the
///     biome and effects.
/// </param>
/// <param name="OverheatsAbove">
///     The heat above which the mech overheats, after the pilot's Guts and effects; the base is in
///     <see cref="HeatRules.OverheatsAbove" />.
/// </param>
/// <param name="MaxHeat">
///     The heat at which the mech shuts down, after heat banks and effects; the base is in
///     <see cref="HeatRules.MaxHeat" />.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechHeat(int Current, int Dissipation, int OverheatsAbove, int MaxHeat);

/// <summary>A unit's pilot's condition in the battle.</summary>
/// <param name="Injuries">The injuries, including those from before the mission.</param>
/// <param name="Health">
///     The injuries the pilot can take before being incapacitated, as in <see cref="LanceUnitOutcome.Health" />.
/// </param>
/// <param name="BonusHealth">
///     The bonus health left, which takes hits before they become injuries and is used up by them; the portrait
///     shows it on top of <paramref name="Health" />.
/// </param>
/// <param name="Skills">
///     The skill levels the game computes with now, temporary changes from effects included; exported for every
///     pilot in full view, though the HUD shows only the player's.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatPilotState(int Injuries, int Health, int BonusHealth, Skills Skills);

/// <summary>A location's armor and structure left, in whole points as the paper doll shows them.</summary>
/// <param name="Location">
///     The game's name of the location: a mech's as in <see cref="ChassisLocationDefinition.Location" />, a
///     vehicle's as in <see cref="VehicleLocationDefinition.Location" />, or <c>Structure</c> for a turret's single
///     location.
/// </param>
/// <param name="Armor">The armor left, by side; only a mech's torso has a rear.</param>
/// <param name="Structure">The structure left; 0 once the location is destroyed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record UnitLocation(string Location, LocationArmor<int> Armor, int Structure);

/// <summary>The unit's guard, which the HUD shows as guarded at <see cref="Level" /> 1 or more.</summary>
/// <param name="Level">
///     What the sources below add up to: bracing and cover 1 each, Bulwark 1 more on top of either. Bracing and
///     Bulwark don't count while the guard is broken (unsteady, prone, shut down, sensor locked or hit), and none
///     while the unit moves.
/// </param>
/// <param name="IsBraced">Braced in its last activation, until its next one.</param>
/// <param name="HasCover">Standing in a terrain that grants guard.</param>
/// <param name="HasBulwark">The pilot has Bulwark.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Guard(int Level, bool IsBraced, bool HasCover, bool HasBulwark);

/// <summary>A component mounted on a unit, with its condition in the battle.</summary>
/// <param name="Location">The game's name of the location it is mounted in, as <see cref="UnitLocation.Location" />.</param>
/// <param name="ShotsLeft">
///     The shots left in an ammunition box, or in a weapon that carries its own; <c>null</c> when it's another
///     component. A destroyed box has none.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatComponent(
    ComponentReference Component,
    string Location,
    ComponentDamageLevel DamageLevel,
    int? ShotsLeft) : ComponentEntry(Component), IDamageable;

/// <summary>An ability a unit can activate, as its HUD button shows it.</summary>
/// <param name="Ability">The ability, the pilot's or a mounted component's.</param>
/// <param name="Cooldown">The unit's activations until the ability can be used again; 0 when ready.</param>
/// <param name="UsesLeft">The uses left of an ability that can be used only so often; <c>null</c> when unlimited.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatAbility(DefinitionReference Ability, int Cooldown, int? UsesLeft);

/// <summary>
///     How a unit could fire at a target from where it stands, as hovering over the target shows it.
/// </summary>
/// <param name="Target">The target's unit id, a key of <see cref="CombatState.Units" />.</param>
/// <param name="Blocking">
///     How much of the line of fire itself is blocked. A target beyond every weapon's range and the unit's sensors is
///     blocked too.
/// </param>
/// <param name="Fire">
///     Whether the unit's weapons could fire at the target directly, only indirectly or not at all, or it is out of
///     their range.
/// </param>
/// <param name="IsInFiringArc">Whether the target is in the unit's firing arc without turning.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LineOfFire(
    string Target,
    LineOfFireBlocking Blocking,
    FireAvailability Fire,
    bool IsInFiringArc);
