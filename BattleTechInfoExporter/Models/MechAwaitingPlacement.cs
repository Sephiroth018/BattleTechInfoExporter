using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A new mech the game asks the player to place, store or scrap because every mech bay is full; it is in
///     neither the mech bay nor storage until then.
/// </summary>
/// <param name="Loadout">
///     The loadout it comes with: the stock one for a mech assembled from parts or bought, none but the fixed
///     components for a salvaged chassis.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechAwaitingPlacement(DefinitionReference Chassis, MechLoadout Loadout);
