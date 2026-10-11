namespace BattleTechInfoExporter.Models;

/// <summary>An entry of an item collection.</summary>
internal interface IItemCollectionEntry
{
    /// <summary>The entry's draw weight among the collection's entries.</summary>
    int Weight { get; }
}
