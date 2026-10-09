using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     How a mission's salvage is made. The picks are the contract's salvage potential times the negotiated share
///     plus <see cref="FloorBonus" />, times the outcome's chance less its loss per mech the company lost; the
///     priority picks are <see cref="PriorityShare" /> of them, at most <see cref="MaxPriorityPicks" />. The rest
///     is drawn from the pool by <see cref="PickWeights" />. A destroyed enemy mech yields its parts by how it died:
///     1 for a destroyed center torso, 2 for both legs, 3 for the head or an incapacitated pilot.
/// </summary>
/// <param name="DefaultPotential">The salvage potential of a contract that sets none.</param>
/// <param name="Victory">For a mission won.</param>
/// <param name="Retreat">For a mission the company retreated from.</param>
/// <param name="Defeat">For a mission failed.</param>
/// <param name="PriorityShare">The share of the picks that are priority picks, a 0-1 fraction, rounded down.</param>
/// <param name="MaxPriorityPicks">The most priority picks a mission gives, hardcoded by the game.</param>
/// <param name="FloorBonus">
///     Picks added to the negotiated share of a contract's salvage potential, unless the potential is 0.
/// </param>
/// <param name="PickWeights">The relative weights the picks are drawn from the pool by.</param>
/// <param name="RareSwaps">The chance of a salvaged component being swapped for a rarer one.</param>
/// <param name="CulledComponentRarity">Components of this rarity or above never come as salvage.</param>
/// <param name="DestroyedMechRecoveryChance">
///     The chance of getting back a lost mech whose center torso was destroyed; a mech with its center torso
///     intact always comes back.
/// </param>
/// <param name="AssembledMechsComeEquipped">
///     Whether a mech assembled from parts or bought comes with its stock loadout instead of only its fixed
///     components.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SalvageRules(
    int DefaultPotential,
    SalvageOutcome Victory,
    SalvageOutcome Retreat,
    SalvageOutcome Defeat,
    float PriorityShare,
    int MaxPriorityPicks,
    int FloorBonus,
    SalvagePickWeights PickWeights,
    RareSalvageSwaps RareSwaps,
    int CulledComponentRarity,
    float DestroyedMechRecoveryChance,
    bool AssembledMechsComeEquipped);

/// <param name="Chance">Multiplies the picks, as a 0-1 fraction.</param>
/// <param name="ChanceLostPerMechDestroyed">
///     Taken off <see cref="Chance" /> for each of the company's mechs destroyed or whose pilot was incapacitated,
///     down to 0.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SalvageOutcome(float Chance, float ChanceLostPerMechDestroyed);

/// <param name="Weapon">The weight of a weapon.</param>
/// <param name="Component">The weight of any other component.</param>
/// <param name="MechPart">The weight of a mech part.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SalvagePickWeights(int Weapon, int Component, int MechPart);

/// <param name="Weapons">For weapons, which keep their kind.</param>
/// <param name="Upgrades">For other components.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RareSalvageSwaps(RareSalvageSwap Weapons, RareSalvageSwap Upgrades);

/// <summary>
///     The chance of a swap is the contract's difficulty plus the tier's <c>ChanceOffset</c>, divided by
///     <see cref="Divisor" />; the very rare tier is rolled first.
/// </summary>
/// <param name="VeryRare">The very rare tier, rolled first.</param>
/// <param name="Rare">
///     The rare tier, compared with the same roll, so it applies when the roll is below its chance but not the very
///     rare tier's.
/// </param>
/// <param name="Divisor">Divides the contract's difficulty plus a tier's offset, giving the tier's 0-1 chance.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RareSalvageSwap(RareSalvageTier VeryRare, RareSalvageTier Rare, float Divisor);

/// <param name="Rarities">The rarities the swapped-in component is drawn from.</param>
/// <param name="ChanceOffset">Added to the contract's difficulty before <see cref="RareSalvageSwap.Divisor" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record RareSalvageTier(IReadOnlyList<float> Rarities, float ChanceOffset);
