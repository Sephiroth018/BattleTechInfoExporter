using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     Where a player's unit can end its move, read from the game's own pathing, with the evasion pips it gets and
///     its lines of fire there; <see cref="CombatMap.Hexes" /> give each hex's terrain and elevation. A unit that has
///     activated gets those of its next activation, from where it stands. The facing after a move isn't exported, as
///     the unit can turn at its end.
/// </summary>
/// <param name="CanSprint">Whether the unit can sprint; a legged or unsteady mech can't.</param>
/// <param name="CanJump">Whether the unit can jump: a mech with a working jump jet.</param>
/// <param name="Hexes">The hexes some move of the unit ends on, by <c>r</c>, then <c>q</c>.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record UnitMovement(bool CanSprint, bool CanJump, IReadOnlyList<ReachableHex> Hexes);

/// <summary>
///     A hex a move of the unit can end on. Each move's evasion pips follow from the length of the path the unit
///     would walk there, or a jump's straight distance, and abilities can raise them above the rules' table.
/// </summary>
/// <param name="Walk">The evasion pips a walk ending on the hex gives; <c>null</c> where no walk reaches it.</param>
/// <param name="Sprint">
///     The evasion pips a sprint ending on the hex gives; <c>null</c> where no sprint reaches it or the unit can't
///     sprint.
/// </param>
/// <param name="Reverse">
///     The evasion pips moving backward to the hex gives; <c>null</c> where no backward move reaches it.
/// </param>
/// <param name="Jump">
///     The evasion pips a jump to the hex gives, one for a jump in place; <c>null</c> where no jump reaches it or the
///     unit can't jump.
/// </param>
/// <param name="LinesOfFire">
///     From the hex to each enemy in full view (<see cref="UnitVisibility.Full" />) the unit has a line of fire to or
///     could fire at indirectly, in the order of their unit ids; an enemy it could do neither to from there has no
///     entry.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ReachableHex(
    int Q,
    int R,
    int? Walk,
    int? Sprint,
    int? Reverse,
    int? Jump,
    IReadOnlyList<MoveLineOfFire> LinesOfFire) : HexCoordinates(Q, R);

/// <summary>How the unit could fire at a target after moving to a hex, as the HUD's move preview shows it.</summary>
/// <param name="Side">The side of the target an attack from the hex hits.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MoveLineOfFire(
    string Target,
    LineOfFireBlocking Blocking,
    FireAvailability Fire,
    float Distance,
    float ElevationModifier,
    AttackSide Side) : LineOfFire(Target, Blocking, Fire, Distance, ElevationModifier);
