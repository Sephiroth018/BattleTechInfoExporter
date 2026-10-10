using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The next financial report, as the finance screen projects it.</summary>
/// <param name="DueOnDay">The day the report is due, on the scale of <see cref="Company.DaysPassed" />.</param>
/// <param name="Spending">The spending level, and what the report would deduct at each level.</param>
/// <param name="ExpectedExpenses">What the report deducts at the current spending level, line by line.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record FinancialReport(
    int DueOnDay,
    Spending Spending,
    ExpectedExpenses ExpectedExpenses);
