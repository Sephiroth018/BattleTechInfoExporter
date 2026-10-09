using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The campaign rules: the constants behind the mech lab, the med bay, hiring, salvage, finances, contracts,
///     alliances and travel, as the game uses them with the career's difficulty settings.
/// </summary>
/// <param name="MechLab">What mech lab work costs and how long it takes.</param>
/// <param name="MedBay">How long pilots take to heal and how likely an incapacitated pilot dies.</param>
/// <param name="Hiring">Hiring and paying pilots, and the roster and mech bay sizes the ship's pods allow.</param>
/// <param name="Salvage">How a mission's salvage is made.</param>
/// <param name="Finances">
///     The financial reports and their expenses, jump costs, store and scrap shares, debt limit and morale changes.
/// </param>
/// <param name="Contracts">How contracts are generated, paid and rewarded.</param>
/// <param name="Alliances">Allying with factions and breaking alliances.</param>
/// <param name="Travel">How long a trip takes and how far a jump reaches.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CampaignRules(
    MechLabRules MechLab,
    MedBayRules MedBay,
    HiringRules Hiring,
    SalvageRules Salvage,
    FinancesRules Finances,
    ContractRules Contracts,
    AllianceRules Alliances,
    TravelRules Travel);
