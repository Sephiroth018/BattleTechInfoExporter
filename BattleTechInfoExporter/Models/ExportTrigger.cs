namespace BattleTechInfoExporter.Models;

/// <summary>What caused an export.</summary>
internal enum ExportTrigger
{
    CareerLoaded,
    ManualSave,
    Autosave,
    ContractCompleted,
    FinancialReportShown,
    MechLabOrderCompleted,
    PilotHealed,
    ShipUpgradeCompleted,
    ContractsGenerated,
    StoreClosed,
    PurchaseCompleted,
    SaleCompleted,
    MechBayChanged,
    ExperienceSpent,
    MechPlacementShown,
    MissionCompleted,
    SalvageChosen,
    ShipUpgradeStarted,
    PhaseStarted,
    CombatStarted,
    CombatLoaded,
    ActivationCompleted
}
