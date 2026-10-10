using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Name">The company's name, as the player chose it.</param>
/// <param name="Dropship">The company's ship: the Leopard at the start, later the Argo.</param>
/// <param name="DaysPassed">
///     The days passed since the career began; every day number in the export (e.g. <c>readyOnDay</c>) is on this
///     scale.
/// </param>
/// <param name="CurrentDate">The in-game calendar date, as <c>yyyy-MM-dd</c>.</param>
/// <param name="Morale">The company's morale, with its level in <see cref="Rules.MoraleLevels" />.</param>
/// <param name="Funds">The company's C-Bills.</param>
/// <param name="MercenaryReviewBoard">The company's standing with the Mercenary Review Board.</param>
/// <param name="Reputation">The reputation with each faction that gains it, except the Mercenary Review Board.</param>
/// <param name="MechTech">
///     The tech points the mech lab works a day (<see cref="CampaignRules.MechLab" />), including
///     <paramref name="TemporaryMechTechChanges" />.
/// </param>
/// <param name="TemporaryMechTechChanges">The changes events made to <paramref name="MechTech" /> for a time, by end day.</param>
/// <param name="MedTech">
///     The med tech skill, which adds heal points for each injured pilot a day (<see cref="CampaignRules.MedBay" />),
///     including <paramref name="TemporaryMedTechChanges" />.
/// </param>
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
    int MaxPilots);
