using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>What the company keeps in storage, outside its mechs.</summary>
/// <param name="Chassis">The stored chassis, which come without weapons or other removable components.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Storage(
    IReadOnlyList<StoredComponent> Components,
    IReadOnlyList<StoredChassis> Chassis,
    IReadOnlyList<StoredMechParts> MechParts);

/// <param name="DamagedCount">The damaged copies, which need a repair once mounted.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StoredComponent(ComponentReference Component, int Count, int DamagedCount)
    : ComponentEntry(Component), IStored;

/// <summary>
///     A stored chassis, described in the catalog. Readying it gives a mech with the stock armor and the chassis'
///     fixed components, but no weapons or other components.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StoredChassis(DefinitionReference Chassis, int Count) : IStored;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StoredMechParts(DefinitionReference Mech, int Count) : MechPartsEntry(Mech), IStored;
