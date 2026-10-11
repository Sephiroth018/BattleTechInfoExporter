namespace BattleTechInfoExporter.Models;

/// <summary>An item an item collection's draw can yield, as opposed to another collection.</summary>
internal interface IItemCollectionItem : IItemCollectionEntry
{
    /// <summary>The copies a draw yields; <c>null</c>: unlimited.</summary>
    int? Count { get; }
}
