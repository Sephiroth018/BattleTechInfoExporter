using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>Salvage items, counted like <see cref="Storage" />.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SalvageItems(
    IReadOnlyList<SalvagedComponent> Components,
    IReadOnlyList<SalvagedMechParts> MechParts);

/// <param name="Count">The working copies.</param>
/// <param name="DamagedCount">The damaged copies; the game generates none.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SalvagedComponent(ComponentReference Component, int Count, int DamagedCount);

/// <summary>
///     Parts of a mech; with <see cref="Rules.MechPartsPerMech" /> of them in storage they become that mech.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SalvagedMechParts(DefinitionReference Mech, int Count);
