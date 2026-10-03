using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Finances(
    int Funds,
    int DaysUntilReport,
    Spending Spending,
    ExpectedExpenses ExpectedExpenses);
