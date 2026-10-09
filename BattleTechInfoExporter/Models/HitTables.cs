using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     Which location a hit lands on: relative weights per location by the direction the attack comes from, in the
///     target's four quadrants, or against a prone mech from any side.
/// </summary>
/// <param name="Mech">The weights of a mech's armor locations.</param>
/// <param name="Vehicle">The weights of a vehicle's locations.</param>
/// <param name="CalledShot">How a called shot raises the chosen location's weight.</param>
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

/// <param name="FromFront">The weight of each location for an attack from the front.</param>
/// <param name="FromLeft">The weight of each location for an attack from the left.</param>
/// <param name="FromRight">The weight of each location for an attack from the right.</param>
/// <param name="FromBack">The weight of each location for an attack from behind.</param>
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
/// <param name="WeightMultiplier">Multiplies the chosen location's weight, before the pilot's own multiplier.</param>
/// <param name="DegradePerHit">
///     The share of the way to 1, from 0 to 1, the multiplier moves after each hit.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CalledShotRules(int WeightMultiplier, float DegradePerHit);

/// <param name="SameLocation">For the location the first hit landed on.</param>
/// <param name="AdjacentLocation">For the locations adjacent to the first hit's.</param>
/// <param name="OtherLocation">For every other location.</param>
/// <param name="IsHeadMultiplied">Whether the head's weight is multiplied like the other locations'.</param>
/// <param name="CanHeadBeClustered">Whether a later hit can land on the head unless the first hit did.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ClusteredHitWeights(
    float SameLocation,
    float AdjacentLocation,
    float OtherLocation,
    bool IsHeadMultiplied,
    bool CanHeadBeClustered);
