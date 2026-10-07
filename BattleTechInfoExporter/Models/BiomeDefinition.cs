using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A biome's effects on every unit on its maps, on top of the terrain's (see <see cref="TerrainDefinition" />);
///     the game reads nothing else from a biome.
/// </summary>
/// <param name="Name">As in the references to the biome.</param>
/// <param name="HeatSinkMultiplier">Multiplies every mech's heat sink capacity.</param>
/// <param name="HeatPerTurn">Added to every mech's heat at the end of its activation.</param>
/// <param name="DamageDealt">Multiplies every unit's weapon damage, by weapon category.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record BiomeDefinition(
    string Name,
    float HeatSinkMultiplier,
    int HeatPerTurn,
    DamageMultipliers DamageDealt);
