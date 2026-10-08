namespace BattleTechInfoExporter.Models;

/// <summary>An entry of the company's storage, or of salvage, which ends up there.</summary>
internal interface IStored
{
    /// <summary>The copies kept; for components, the working ones.</summary>
    int Count { get; }
}
