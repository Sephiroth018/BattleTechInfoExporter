using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The hexes a player's unit can end its move on, read from the game's own pathing, with the evasion pips it gets
///     and the attacks it has there. A unit that has activated gets those of its next activation, from where it
///     stands. Laid out in rows of characters, as <see cref="MovementLegend" /> explains, like
///     <see cref="CombatMap.Rows" />, whose hexes give each hex's terrain and elevation.
/// </summary>
/// <param name="Targets">
///     The enemies in full view (<see cref="UnitVisibility.Full" />) by unit id, in the order of
///     <see cref="MovementRow.Attacks" />.
/// </param>
/// <param name="Rows">The rows with a hex the unit can reach, by <c>r</c>.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record UnitMovement(IReadOnlyList<string> Targets, IReadOnlyList<MovementRow> Rows);

/// <summary>
///     Consecutive hexes of one row, from (<see cref="Q" />, <see cref="R" />) to the last one the unit can reach,
///     one character per hex in each move and two per hex in each attack.
/// </summary>
/// <param name="Q">The axial <c>q</c> of the row's first hex, the first character of each move and attack.</param>
/// <param name="R">The axial <c>r</c> of every hex in the row.</param>
/// <param name="Walk">The evasion pips per hex as <see cref="MovementLegend.Moves" /> says.</param>
/// <param name="Sprint">
///     Like <see cref="Walk" />, when sprinting; <c>null</c> when the unit is a legged or unsteady mech, which can't
///     sprint.
/// </param>
/// <param name="Reverse">Like <see cref="Walk" />, when moving backward.</param>
/// <param name="Jump">
///     Like <see cref="Walk" />, when jumping; <c>null</c> when the unit is a vehicle or a mech without a working
///     jump jet.
/// </param>
/// <param name="Attacks">
///     Per entry of <see cref="UnitMovement.Targets" />, the fire and side as <see cref="MovementLegend.Fire" /> and
///     <see cref="MovementLegend.Side" /> say.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MovementRow(
    int Q,
    int R,
    string Walk,
    string? Sprint,
    string Reverse,
    string? Jump,
    IReadOnlyList<string> Attacks);

/// <summary>How to read <see cref="UnitMovement" />'s rows, spelled out so the file explains itself.</summary>
/// <param name="Moves">How to read the characters of the moves.</param>
/// <param name="Fire">The first character of an attack: how the unit could fire at the target from the hex.</param>
/// <param name="Side">The second character of an attack: the side of the target an attack from the hex hits.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MovementLegend(
    string Moves,
    IReadOnlyDictionary<string, string> Fire,
    IReadOnlyDictionary<string, string> Side);
