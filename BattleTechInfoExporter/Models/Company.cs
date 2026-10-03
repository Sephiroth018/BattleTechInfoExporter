using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Company(
    string Name,
    int Funds,
    int MonthlyExpenses,
    int DaysPassed,
    string CurrentDate,
    MercenaryReviewBoard MercenaryReviewBoard,
    IReadOnlyList<FactionReputation> Reputation);
