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
    int MercenaryReviewBoardLevel,
    IReadOnlyList<FactionReputation> Reputation);
