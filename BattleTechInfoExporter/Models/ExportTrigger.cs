using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BattleTechInfoExporter.Models;

/// <summary>What caused an export; its names are written to the file in camelCase.</summary>
[JsonConverter(typeof(StringEnumConverter), true)]
internal enum ExportTrigger
{
    CareerLoaded,
    ManualSave,
    Autosave,
    ContractCompleted,
    MonthlyExpensesPaid
}
