namespace BattleTechInfoExporter.Models;

/// <summary>A definition in the catalog, keyed by its id in its root object.</summary>
internal interface ICatalogEntry
{
    /// <summary>The id from the game's data files, which the entry is keyed by and the references to it use.</summary>
    string Id { get; }
}
