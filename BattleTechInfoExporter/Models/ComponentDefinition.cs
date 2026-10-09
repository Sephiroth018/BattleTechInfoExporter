using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The fields every component definition has; the definitions of types with stats of their own derive from
///     it. Upgrades have no others.
/// </summary>
/// <param name="Name">The name the mech lab shows, as in the references to the component.</param>
/// <param name="Tonnage">The component's weight in tons.</param>
/// <param name="Slots">The slots the component takes up in the location it's mounted in.</param>
/// <param name="Cost">The C-Bill value of the component.</param>
/// <param name="Bonuses">The short bonus texts shown on the component, e.g. "+ 5 Dmg.".</param>
/// <param name="Effects">The statistics the component changes while mounted.</param>
/// <param name="IsSalvageable">
///     Whether the component can come as salvage; the game's <c>BLACKLISTED</c> tag rules it out. Nothing else
///     follows from it, e.g. about stores.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal record ComponentDefinition(
    string Name,
    float Tonnage,
    int Slots,
    int Cost,
    IReadOnlyList<string> Bonuses,
    IReadOnlyList<StatisticChange> Effects,
    bool IsSalvageable);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record WeaponDefinition : ComponentDefinition
{
    internal WeaponDefinition(
        ComponentDefinition component,
        string category,
        string? ammoCategory,
        int? internalAmmoCapacity,
        float damage,
        float instability,
        int shotsWhenFired,
        int projectilesPerShot,
        float heatDamage,
        int heatGenerated,
        WeaponRanges ranges,
        float accuracyModifier,
        float criticalChanceMultiplier,
        int refireModifier,
        bool isIndirectFireCapable) : base(component)
    {
        Category = category;
        AmmoCategory = ammoCategory;
        InternalAmmoCapacity = internalAmmoCapacity;
        Damage = damage;
        Instability = instability;
        ShotsWhenFired = shotsWhenFired;
        ProjectilesPerShot = projectilesPerShot;
        HeatDamage = heatDamage;
        HeatGenerated = heatGenerated;
        Ranges = ranges;
        AccuracyModifier = accuracyModifier;
        CriticalChanceMultiplier = criticalChanceMultiplier;
        RefireModifier = refireModifier;
        IsIndirectFireCapable = isIndirectFireCapable;
    }

    /// <summary>The hardpoint kind the weapon needs, e.g. "Ballistic" or "Support".</summary>
    public string Category { get; }

    /// <summary>
    ///     The ammo the weapon fires, as <see cref="AmmunitionBoxDefinition.AmmoCategory" /> names it; <c>null</c>
    ///     when it needs none.
    /// </summary>
    public string? AmmoCategory { get; }

    /// <summary>
    ///     The shots a weapon that carries its own ammo instead of using ammo boxes, like a flamer, has per mission;
    ///     <c>null</c> for every other weapon.
    /// </summary>
    public int? InternalAmmoCapacity { get; }

    /// <summary>The damage of one shot.</summary>
    public float Damage { get; }

    /// <summary>The stability damage of one shot.</summary>
    public float Instability { get; }

    /// <summary>The shots fired per attack.</summary>
    public int ShotsWhenFired { get; }

    /// <summary>
    ///     The projectiles the attack's animation shows per shot; only the visuals use it, each shot hits or misses
    ///     whole.
    /// </summary>
    public int ProjectilesPerShot { get; }

    /// <summary>The heat the target takes per shot.</summary>
    public float HeatDamage { get; }

    /// <summary>The heat the mech takes per attack.</summary>
    public int HeatGenerated { get; }

    /// <summary>The range brackets, which decide the to-hit modifier by the target's distance.</summary>
    public WeaponRanges Ranges { get; }

    /// <summary>The weapon's own to-hit modifier, which the hit chance adds to the others.</summary>
    public float AccuracyModifier { get; }

    /// <summary>Multiplies the chance of the weapon's hits to cause a critical hit; 1 leaves it unchanged.</summary>
    public float CriticalChanceMultiplier { get; }

    /// <summary>The accuracy penalty for firing again in the next round.</summary>
    public int RefireModifier { get; }

    /// <summary>Whether the weapon can fire at targets out of line of sight.</summary>
    public bool IsIndirectFireCapable { get; }
}

/// <summary>The range brackets in meters, each with its to-hit modifier in <see cref="RangeBandModifiers" />.</summary>
/// <param name="Min">
///     The minimum range; closer, the weapon still fires, with <see cref="RangeBandModifiers.WithinMinimumRange" />.
/// </param>
/// <param name="Short">
///     The end of the short bracket, from <see cref="Min" />, where <c>rules.json</c>'s
///     <see cref="RangeBandModifiers.Short" /> applies.
/// </param>
/// <param name="Medium">
///     The end of the medium bracket, from <see cref="Short" />, where <c>rules.json</c>'s
///     <see cref="RangeBandModifiers.Medium" /> applies.
/// </param>
/// <param name="Long">
///     The end of the long bracket, from <see cref="Medium" />, where <c>rules.json</c>'s
///     <see cref="RangeBandModifiers.Long" /> applies.
/// </param>
/// <param name="Max">
///     The end of the maximum bracket, from <see cref="Long" />, where <c>rules.json</c>'s
///     <see cref="RangeBandModifiers.Maximum" /> applies; the weapon can't fire beyond it.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record WeaponRanges(float Min, float Short, float Medium, float Long, float Max);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record AmmunitionBoxDefinition : ComponentDefinition
{
    internal AmmunitionBoxDefinition(ComponentDefinition component, string ammoCategory, int capacity) : base(component)
    {
        AmmoCategory = ammoCategory;
        Capacity = capacity;
    }

    /// <summary>The weapons the ammo fits, e.g. "AC/5".</summary>
    public string AmmoCategory { get; }

    /// <summary>The shots in the box; it is full at the start of every mission.</summary>
    public int Capacity { get; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HeatSinkDefinition : ComponentDefinition
{
    internal HeatSinkDefinition(ComponentDefinition component, float dissipation) : base(component)
    {
        Dissipation = dissipation;
    }

    /// <summary>The heat the heat sink removes per round.</summary>
    public float Dissipation { get; }
}

/// <summary>A jump jet, which only mechs whose chassis tonnage is within its range can mount.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record JumpJetDefinition : ComponentDefinition
{
    internal JumpJetDefinition(ComponentDefinition component, float minTonnage, float maxTonnage) : base(component)
    {
        MinTonnage = minTonnage;
        MaxTonnage = maxTonnage;
    }

    /// <summary>The lowest chassis tonnage that can mount the jump jet, inclusive.</summary>
    public float MinTonnage { get; }

    /// <summary>The highest chassis tonnage that can mount the jump jet, inclusive.</summary>
    public float MaxTonnage { get; }
}
