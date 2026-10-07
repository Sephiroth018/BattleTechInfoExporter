using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A unit in combat, with as much of it as the HUD shows at its <see cref="Visibility" />.</summary>
/// <param name="Faction">The faction of the unit's team.</param>
/// <param name="Kind">
///     The kind of unit; <c>null</c> where the HUD doesn't tell (<see cref="UnitVisibility.BlipMinimum" />
///     ).
/// </param>
/// <param name="Tonnage">
///     A blip's tonnage, which the HUD shows at <see cref="UnitVisibility.BlipMaximum" /> for mechs and vehicles;
///     otherwise <c>null</c>, as the definition has it.
/// </param>
/// <param name="WeightClass">A blip's weight class, which the HUD shows instead of the tonnage for turrets.</param>
/// <param name="Definition">
///     The mech, vehicle or turret in the catalog, which holds its armor and loadout; <c>null</c> for the player's
///     mechs, which have <see cref="BayMech" /> instead, and for blips.
/// </param>
/// <param name="BayMech">The player's mech in <c>game-state.json</c>, whose loadout holds its assigned armor.</param>
/// <param name="Position">Where the unit is, or was last detected at for <see cref="UnitVisibility.LastSeen" />.</param>
/// <param name="Facing">
///     The direction the unit faces, in degrees clockwise from the map's +Z axis; <c>null</c> when last seen.
/// </param>
/// <param name="Terrain">
///     The terrain the unit stands in, keyed as in the catalog's <see cref="Catalog.TerrainDefinitions" />;
///     <c>null</c> on open ground and when last seen.
/// </param>
/// <param name="Pilot">The pilot, which the HUD names only at <see cref="UnitVisibility.Full" />.</param>
/// <param name="State">The unit's condition, shown at <see cref="UnitVisibility.Full" />.</param>
/// <param name="LinesOfFire">
///     The lines of fire to the units it could attack: from the player's units to every enemy at
///     <see cref="UnitVisibility.Full" />, and from those enemies to the player's units. <c>null</c> for the others.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatUnit(
    DefinitionReference Faction,
    UnitAllegiance Allegiance,
    UnitVisibility Visibility,
    UnitKind? Kind,
    float? Tonnage,
    WeightClass? WeightClass,
    DefinitionReference? Definition,
    MechReference? BayMech,
    MapPosition Position,
    float? Facing,
    string? Terrain,
    PilotReference? Pilot,
    CombatUnitState? State,
    IReadOnlyList<LineOfFire>? LinesOfFire);

/// <param name="Locations">
///     The armor and structure left per location, in the game's order: a mech's from head to legs, a vehicle's from
///     front to rear and turret, a turret's single one. The limits are in <see cref="CombatUnit.Definition" /> or
///     <see cref="CombatUnit.BayMech" />.
/// </param>
/// <param name="Heat"><c>null</c> for vehicles and turrets, which have none.</param>
/// <param name="Stability"><c>null</c> for vehicles and turrets, which have none.</param>
/// <param name="Initiative">The phase the unit acts in, on the scale of <see cref="CombatState.Phase" />.</param>
/// <param name="Components">Every component mounted, in the game's order.</param>
/// <param name="Abilities">The pilot's and components' abilities that can be activated, with their cooldowns.</param>
/// <param name="PrecisionStrikeCost">The resolve a Precision Strike costs the unit now, which its pilot's morale changes.</param>
/// <param name="VigilanceCost">The resolve Vigilance costs the unit now.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatUnitState(
    bool IsDestroyed,
    IReadOnlyList<UnitLocation> Locations,
    int EvasionPips,
    Guard Guard,
    bool IsEntrenched,
    bool IsProne,
    bool IsShutDown,
    bool IsUnsteady,
    int? Heat,
    float? Stability,
    bool HasActivated,
    int Initiative,
    IReadOnlyList<CombatComponent> Components,
    IReadOnlyList<CombatAbility> Abilities,
    int PrecisionStrikeCost,
    int VigilanceCost);

/// <summary>A location's armor and structure left, in whole points as the paper doll shows them.</summary>
/// <param name="Location">The game's name of the location (a mech's, vehicle's or turret's).</param>
/// <param name="RearArmor"><c>null</c> outside a mech's torso, which alone has rear armor.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record UnitLocation(string Location, int Armor, int? RearArmor, int Structure);

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

/// <param name="Location">The game's name of the location it is mounted in.</param>
/// <param name="Ammo">
///     The rounds left in an ammunition box, or in a weapon that carries its own; <c>null</c> for other components.
///     A destroyed box has none.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatComponent(
    ComponentReference Component,
    string Location,
    ComponentDamageLevel DamageLevel,
    int? Ammo);

/// <param name="Cooldown">The unit's activations until the ability can be used again; 0 when ready.</param>
/// <param name="UsesLeft">The uses left of an ability that can be used only so often; <c>null</c> without a limit.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatAbility(DefinitionReference Ability, int Cooldown, int? UsesLeft);

/// <summary>
///     How a unit could fire at a target from where it stands, as hovering over the target shows it.
/// </summary>
/// <param name="Target">The target's unit id, a key of <see cref="CombatState.Units" />.</param>
/// <param name="Level">
///     The line of fire itself: clear, obstructed (the target has cover) or blocked. A target beyond every weapon's
///     range and the unit's sensors is blocked too.
/// </param>
/// <param name="IsInFiringArc">Whether the target is in the unit's firing arc without turning.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LineOfFire(
    string Target,
    LineOfFireLevel Level,
    FireAvailability Fire,
    bool IsInFiringArc);
