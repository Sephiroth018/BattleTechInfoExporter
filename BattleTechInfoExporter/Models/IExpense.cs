namespace BattleTechInfoExporter.Models;

/// <summary>A line of the expenses of a financial report.</summary>
internal interface IExpense
{
    /// <summary>The C-Bills the line deducts, rounded like on the game's finance screen.</summary>
    int Amount { get; }
}
