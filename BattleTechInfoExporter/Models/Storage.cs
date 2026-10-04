using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>What the company keeps in storage, outside its mechs.</summary>
/// <param name="Mechs">The stored chassis, which come without equipment.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Storage(
    IReadOnlyList<StoredComponent> Equipment,
    IReadOnlyList<StoredChassis> Mechs,
    IReadOnlyList<StoredMechParts> MechParts);

/// <param name="Count">The working copies.</param>
/// <param name="DamagedCount">The damaged copies, which need a repair once mounted.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StoredComponent(ComponentReference Component, int Count, int DamagedCount);

/// <summary>
///     A stored chassis. Readying it gives a mech with the stock armor and the chassis' fixed equipment, but no
///     weapons or other equipment.
/// </summary>
/// <param name="Role">The chassis' stock role, e.g. "Brawler".</param>
/// <param name="Hardpoints">The weapon hardpoints of all locations together.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StoredChassis(
    DefinitionReference Chassis,
    WeightClass WeightClass,
    string Role,
    float Tonnage,
    Hardpoints Hardpoints,
    int MaxJumpJets,
    int Count);

/// <summary>
///     Salvaged parts of a mech; with <see cref="Rules.MechPartsPerMech" /> of them they become that mech.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StoredMechParts(DefinitionReference Mech, int Count);
