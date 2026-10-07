using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="NextReportOnDay">
///     The day of the next financial report, on the scale of <see cref="Company.DaysPassed" />.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record FinancialReport(
    int Funds,
    int NextReportOnDay,
    Spending Spending,
    ExpectedExpenses ExpectedExpenses);
