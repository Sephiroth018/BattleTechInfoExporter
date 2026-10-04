using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

// Every definition starts with the same fields as ComponentDefinition: Name, Tonnage, Slots, Cost and Bonuses.

/// <summary>The fields every component definition has; jump jets and upgrades have no others.</summary>
/// <param name="Name">The name the mech lab shows, as in the references to the component.</param>
/// <param name="Cost">The C-Bill value of the component.</param>
/// <param name="Bonuses">The short bonus texts shown on the component, e.g. "+ 5 Dmg.".</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ComponentDefinition(
    string Name,
    float Tonnage,
    int Slots,
    int Cost,
    IReadOnlyList<string> Bonuses);

/// <param name="Category">The hardpoint kind the weapon needs, e.g. "Ballistic" or "Support".</param>
/// <param name="AmmoCategory">
///     The ammo the weapon fires, as <see cref="AmmunitionBoxDefinition.AmmoCategory" /> names it; <c>null</c>
///     when it needs none.
/// </param>
/// <param name="Damage">The damage of one shot.</param>
/// <param name="Instability">The stability damage of one shot.</param>
/// <param name="ShotsWhenFired">The shots fired per attack.</param>
/// <param name="HeatDamage">The heat the target takes per shot.</param>
/// <param name="HeatGenerated">The heat the mech takes per attack.</param>
/// <param name="AccuracyModifier">The weapon's own to-hit modifier, which the hit chance adds to the others.</param>
/// <param name="RefireModifier">The accuracy penalty for firing again in the next round.</param>
/// <param name="IsIndirectFireCapable">Whether the weapon can fire at targets out of line of sight.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record WeaponDefinition(
    string Name,
    float Tonnage,
    int Slots,
    int Cost,
    IReadOnlyList<string> Bonuses,
    string Category,
    string? AmmoCategory,
    float Damage,
    float Instability,
    int ShotsWhenFired,
    int ProjectilesPerShot,
    float HeatDamage,
    int HeatGenerated,
    WeaponRanges Ranges,
    float AccuracyModifier,
    float CriticalChanceMultiplier,
    int RefireModifier,
    bool IsIndirectFireCapable);

/// <summary>The range brackets in meters: the weapon is less accurate below short range and beyond long range.</summary>
/// <param name="Min">Below it the weapon can't fire.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record WeaponRanges(float Min, float Short, float Medium, float Long, float Max);

/// <param name="AmmoCategory">The weapons the ammo fits, e.g. "AC/5".</param>
/// <param name="Capacity">The shots in the box; it is full at the start of every mission.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record AmmunitionBoxDefinition(
    string Name,
    float Tonnage,
    int Slots,
    int Cost,
    IReadOnlyList<string> Bonuses,
    string AmmoCategory,
    int Capacity);

/// <param name="Dissipation">The heat the heat sink removes per round.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HeatSinkDefinition(
    string Name,
    float Tonnage,
    int Slots,
    int Cost,
    IReadOnlyList<string> Bonuses,
    float Dissipation);
