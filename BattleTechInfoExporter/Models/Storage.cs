using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>What the company keeps in storage, outside its mechs.</summary>
/// <param name="Components">The stored components.</param>
/// <param name="Chassis">The stored chassis, which come without weapons or other removable components.</param>
/// <param name="MechParts">The stored mech parts.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Storage(
    IReadOnlyList<StoredComponent> Components,
    IReadOnlyList<StoredChassis> Chassis,
    IReadOnlyList<StoredMechParts> MechParts);

/// <summary>A stored component, described in the catalog.</summary>
/// <param name="Count">The working copies; the damaged ones are in <see cref="DamagedCount" />.</param>
/// <param name="DamagedCount">The damaged copies, which need a repair once mounted.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StoredComponent(ComponentReference Component, int Count, int DamagedCount)
    : ComponentEntry(Component), IStored;

/// <summary>
///     A stored chassis, described in the catalog. Readying it gives a mech with the stock armor and the chassis'
///     fixed components, but no weapons or other components.
/// </summary>
/// <param name="Chassis">The chassis, described in <see cref="Catalog.ChassisDefinitions" />.</param>
/// <param name="Count">The chassis kept.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StoredChassis(DefinitionReference Chassis, int Count) : IStored;

/// <summary>Stored parts of a mech, described in the catalog, from which the mech is assembled.</summary>
/// <param name="Count">The parts kept.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record StoredMechParts(DefinitionReference Mech, int Count) : MechPartsEntry(Mech), IStored;
