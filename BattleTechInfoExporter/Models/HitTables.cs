using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     Which location a hit lands on: relative weights per location by the direction the attack comes from, in the
///     target's four quadrants, or against a prone mech from any side.
/// </summary>
/// <param name="ClusteredHits">
///     Multiplies the weights for the later hits of a weapon that fires several, by their place relative to the
///     first hit.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HitTables(
    HitTablesOf<ArmorLocation> Mech,
    HitTablesOf<VehicleChassisLocations> Vehicle,
    CalledShotRules CalledShot,
    ClusteredHitWeights ClusteredHits);

/// <param name="Prone"><c>null</c> for vehicles, which can't be prone.</param>
/// <param name="Artillery">The locations an artillery strike or mortar hits, each with its own damage.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HitTablesOf<TLocation>(
    IReadOnlyDictionary<TLocation, int> FromFront,
    IReadOnlyDictionary<TLocation, int> FromLeft,
    IReadOnlyDictionary<TLocation, int> FromRight,
    IReadOnlyDictionary<TLocation, int> FromBack,
    IReadOnlyDictionary<TLocation, int>? Prone,
    IReadOnlyList<TLocation> Artillery)
    where TLocation : notnull;

/// <summary>
///     A called shot multiplies the chosen location's weight by <see cref="WeightMultiplier" /> times the pilot's own
///     multiplier; after every hit of the attack the multiplier moves towards 1 by <see cref="DegradePerHit" /> of
///     the way.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CalledShotRules(int WeightMultiplier, float DegradePerHit);

/// <param name="Head">Whether the head's weight is multiplied; when not, it's also only hit again after a head hit.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ClusteredHitWeights(
    float SameLocation,
    float AdjacentLocation,
    float OtherLocation,
    bool Head);
