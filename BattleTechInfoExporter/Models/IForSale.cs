namespace BattleTechInfoExporter.Models;

/// <summary>An entry of a store's stock.</summary>
internal interface IForSale
{
    /// <summary>The copies in stock; <c>null</c>: unlimited.</summary>
    int? Count { get; }

    /// <summary>The C-Bills for one copy, after the system's discount and the reputation's price change.</summary>
    int Price { get; }
}
