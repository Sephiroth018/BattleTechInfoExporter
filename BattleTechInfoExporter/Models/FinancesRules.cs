using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="DaysPerReport">The days between financial reports, which deduct the expenses.</param>
/// <param name="MechCostPerReport">The upkeep of each mech in a mech bay.</param>
/// <param name="ShipMaintenance">The ship's own maintenance in each report's expenses.</param>
/// <param name="SpendingLevels">The spending levels, which <see cref="Spending.Level" /> refers to.</param>
/// <param name="JumpCost">The C-Bills each jump of a trip costs.</param>
/// <param name="SellShare">The share of its value a store pays for a mech or an undamaged component.</param>
/// <param name="SellDamagedShare">The share of its value a store pays for a damaged component.</param>
/// <param name="ScrapShare">The share of the chassis' value scrapping a mech, chassis or mech part pays.</param>
/// <param name="MaxDebt">The company is dissolved when its funds fall below this.</param>
/// <param name="BadFaithMoraleChange">When a mission is failed without a good faith effort.</param>
/// <param name="CatastropheMoraleChange">When every pilot sent on a mission is killed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record FinancesRules(
    int DaysPerReport,
    int MechCostPerReport,
    ShipMaintenance ShipMaintenance,
    IReadOnlyList<SpendingLevelRules> SpendingLevels,
    int JumpCost,
    float SellShare,
    float SellDamagedShare,
    float ScrapShare,
    int MaxDebt,
    int BadFaithMoraleChange,
    int CatastropheMoraleChange);

/// <summary>The ship's own line in the expenses of each report, before the spending level's multiplier.</summary>
/// <param name="Leopard">The C-Bills while the company's ship is the Leopard.</param>
/// <param name="Argo">The C-Bills while the company's ship is the Argo.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ShipMaintenance(int Leopard, int Argo);

/// <param name="Level">The spending level, as the game names it, e.g. <c>Spartan</c>.</param>
/// <param name="CostMultiplier">Multiplies the expenses of each report.</param>
/// <param name="MoraleChange">Applied once when the level is confirmed at a report.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SpendingLevelRules(EconomyScale Level, float CostMultiplier, int MoraleChange);
