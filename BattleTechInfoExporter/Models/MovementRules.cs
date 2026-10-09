using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     What shortens a mech's walk, sprint and backward distance; the penalties add up and the distance keeps at
///     least <see cref="MinMultiplier" />. Jump distance is unaffected (see <see cref="Rules.JumpDistances" />).
/// </summary>
/// <param name="OverheatedPenalty">The share lost while overheated.</param>
/// <param name="LegDamagePenalty">The share lost per leg, by its damage.</param>
/// <param name="MinMultiplier">The least share of the distance a mech keeps, from 0 to 1.</param>
/// <param name="PivotInPlaceThreshold">
///     A move that ends closer than this to where it started, in meters, only turns the mech and costs no move.
/// </param>
/// <param name="HexWidth">The width of the movement grid's hexes in meters.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MovementRules(
    float OverheatedPenalty,
    LegDamagePenalties LegDamagePenalty,
    float MinMultiplier,
    float PivotInPlaceThreshold,
    float HexWidth);

/// <param name="Penalized">The leg is damaged.</param>
/// <param name="NonFunctional">The leg is damaged beyond use but not destroyed.</param>
/// <param name="Destroyed">The leg is destroyed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LegDamagePenalties(float Penalized, float NonFunctional, float Destroyed);
