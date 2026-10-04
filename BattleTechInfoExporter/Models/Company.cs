using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="MaxPilots">The pilots the barracks hold, not counting the commander; no more can be hired.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Company(
    string Name,
    DropshipType Dropship,
    int DaysPassed,
    string CurrentDate,
    Morale Morale,
    Finances Finances,
    MercenaryReviewBoard MercenaryReviewBoard,
    IReadOnlyList<FactionReputation> Reputation,
    int MechTech,
    int MedTech,
    int MaxPilots);
