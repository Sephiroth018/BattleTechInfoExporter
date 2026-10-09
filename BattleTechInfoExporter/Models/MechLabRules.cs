using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     What mech lab work costs. The mech lab works through its orders one at a time, paying the company's
///     <see cref="Company.MechTech" /> tech points a day, so an order takes its tech points divided by that,
///     rounded up, at least a day.
/// </summary>
/// <param name="StructureRepair">Per point of structure.</param>
/// <param name="DestroyedLocationRepairMultipliers">
///     Multiply <see cref="StructureRepair" /> for a location that was destroyed.
/// </param>
/// <param name="ComponentRepair">Per slot of the component; a chassis' fixed components are repaired for free.</param>
/// <param name="Install">Per slot of the component, by its category.</param>
/// <param name="ArmorChange">Per point of armor added or removed.</param>
/// <param name="UninstallTechPointsPerSlot">Removing a component costs no C-Bills.</param>
/// <param name="ReadyMechTechPoints">For readying a stored chassis.</param>
/// <param name="CancelRefundShare">The share of the unpaid work refunded when a started order is cancelled.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechLabRules(
    MechLabCost StructureRepair,
    MechLabCostMultipliers DestroyedLocationRepairMultipliers,
    MechLabCost ComponentRepair,
    InstallCosts Install,
    MechLabCost ArmorChange,
    int UninstallTechPointsPerSlot,
    int ReadyMechTechPoints,
    float CancelRefundShare);

/// <param name="TechPoints">The tech points the work takes, per the unit its use names.</param>
/// <param name="CBills">The C-Bills the work costs, per the same unit.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechLabCost(float TechPoints, float CBills);

/// <param name="TechPoints">Multiplies the tech points.</param>
/// <param name="CBills">Multiplies the C-Bills.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechLabCostMultipliers(float TechPoints, float CBills);

/// <summary>
///     By the component's category: weapons by their type, where ballistic covers autocannons, Gauss rifles and
///     machine guns, missile LRMs and SRMs, energy lasers, PPCs and COILs, and support flamers; other weapons and
///     upgrades are other.
/// </summary>
/// <param name="Ballistic">For autocannons, Gauss rifles and machine guns.</param>
/// <param name="Missile">For LRMs and SRMs.</param>
/// <param name="Energy">For lasers, PPCs and COILs.</param>
/// <param name="Support">For flamers.</param>
/// <param name="Ammunition">For ammunition boxes.</param>
/// <param name="JumpJet">For jump jets.</param>
/// <param name="HeatSink">For heat sinks.</param>
/// <param name="Other">For other weapons and upgrades.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record InstallCosts(
    MechLabCost Ballistic,
    MechLabCost Missile,
    MechLabCost Energy,
    MechLabCost Support,
    MechLabCost Ammunition,
    MechLabCost JumpJet,
    MechLabCost HeatSink,
    MechLabCost Other);
