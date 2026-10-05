namespace BattleTechInfoExporter.Models;

/// <summary>What caused an export.</summary>
internal enum ExportTrigger
{
    CareerLoaded,
    ManualSave,
    Autosave,
    ContractCompleted,
    FinancialReportShown,
    WorkOrderCompleted,
    ContractsGenerated,
    StoreClosed,
    MechBayChanged
}
