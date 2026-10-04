using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A component's definition, shared by every copy of the component.</summary>
/// <param name="Name">The name the mech lab shows, as in the references to the component.</param>
/// <param name="Type">The component's kind; only the matching one of the stats sections is set.</param>
/// <param name="Cost">The C-Bill value of the component.</param>
/// <param name="Bonuses">The short bonus texts shown on the component, e.g. "+ 5 Dmg.".</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ComponentDefinition(
    string Name,
    ComponentType Type,
    float Tonnage,
    int Slots,
    int Cost,
    IReadOnlyList<string> Bonuses,
    WeaponStats? Weapon,
    AmmoBoxStats? AmmoBox,
    HeatSinkStats? HeatSink);

/// <param name="Category">The hardpoint kind the weapon needs, e.g. "Ballistic" or "Support".</param>
/// <param name="AmmoCategory">
///     The ammo the weapon fires, as <see cref="AmmoBoxStats.AmmoCategory" /> names it; <c>null</c>
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
internal sealed record WeaponStats(
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
internal sealed record AmmoBoxStats(string AmmoCategory, int Capacity);

/// <param name="Dissipation">The heat the heat sink removes per round.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HeatSinkStats(float Dissipation);
