using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="TemporaryMechTechChanges">The changes events made to <paramref name="MechTech" /> for a time, by end day.</param>
/// <param name="TemporaryMedTechChanges">The changes events made to <paramref name="MedTech" /> for a time, by end day.</param>
/// <param name="MaxPilots">The pilots the barracks hold, not counting the commander; no more can be hired.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Company(
    string Name,
    DropshipType Dropship,
    int DaysPassed,
    string CurrentDate,
    Morale Morale,
    int Funds,
    MercenaryReviewBoard MercenaryReviewBoard,
    IReadOnlyList<FactionReputation> Reputation,
    int MechTech,
    IReadOnlyList<TemporaryChange> TemporaryMechTechChanges,
    int MedTech,
    IReadOnlyList<TemporaryChange> TemporaryMedTechChanges,
    int MaxPilots)
{
    /// <summary>The company without its current day and date, which change every day on their own.</summary>
    internal Company WithoutDay() => this with { DaysPassed = default, CurrentDate = string.Empty };
}
