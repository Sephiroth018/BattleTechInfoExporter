namespace BattleTechInfoExporter.Models;

/// <summary>What a work queue entry is about, which decides the entry's other fields.</summary>
internal enum WorkQueueEntryKind
{
    /// <summary>A mech lab order on a mech of the company.</summary>
    MechLab,

    /// <summary>A pilot healing in the med bay.</summary>
    MedBay,

    /// <summary>The trip under way.</summary>
    Travel,

    /// <summary>A ship upgrade being installed.</summary>
    ShipUpgrade,

    /// <summary>The next financial report.</summary>
    FinancialReport
}
