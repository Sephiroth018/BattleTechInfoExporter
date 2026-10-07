using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The campaign rules: the constants behind the mech lab, the med bay, hiring, salvage, finances, contracts,
///     alliances and travel, as the game uses them with the career's difficulty settings.
/// </summary>
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
