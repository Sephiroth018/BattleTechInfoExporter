using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Makes the <see cref="DefinitionReference" /> to a game definition that several readers share; references only
///     one section needs are made by the reader of that section.
/// </summary>
internal static class DefinitionReferences
{
    internal static DefinitionReference ReferenceTo(BaseDescriptionDef description) =>
        new(description.Id, description.Name);

    internal static DefinitionReference ReferenceTo(FactionValue faction) =>
        new(faction.FactionDefID, FactionNames.Format(faction));

    // Mirrors LanceHeaderWidget, which names the biome by its description and falls back to the skin's name.
    internal static DefinitionReference ReferenceTo(DataManager dataManager, Biome.BIOMESKIN biome) =>
        new(biome.ToString(),
            dataManager.GetBaseDescriptionDef(biome)?.Name ?? Utilities.BeautifyName(biome.ToString()));
}
