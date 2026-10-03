using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Company(
    string Name,
    DropshipType Dropship,
    int Funds,
    int MonthlyExpenses,
    int DaysPassed,
    string CurrentDate,
    MercenaryReviewBoard MercenaryReviewBoard,
    IReadOnlyList<FactionReputation> Reputation);
