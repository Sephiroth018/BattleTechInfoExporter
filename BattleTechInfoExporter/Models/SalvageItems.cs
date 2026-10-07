using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The root of <c>salvage-received.json</c>: the salvage of the mission in <c>mission-outcome.json</c> once it is
///     final. The file doesn't exist until then.
/// </summary>
/// <param name="Received">
///     Everything the company gets: the priority salvage, the salvage drawn at random and
///     <see cref="SalvageOffer.Automatic" />.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SalvageReceived(
    string ModVersion,
    ExportTrigger Trigger,
    MissionContract Contract,
    SalvageItems Received) : ExportFile(ModVersion, null, Trigger);

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
