using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The damage multipliers that apply regardless of the target. A building takes none of the cover, terrain and
///     damage reduction multipliers, which apply only to units.
/// </summary>
/// <param name="GlobalMultiplier">
///     Multiplies the damage and structure damage of every weapon, melee and jump attacks and mortars included; not
///     of artillery strikes.
/// </param>
/// <param name="ArtilleryAgainstBuildingsMultiplier">
///     Multiplies the damage artillery strikes and mortars deal to a building.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record DamageRules(float GlobalMultiplier, float ArtilleryAgainstBuildingsMultiplier);
