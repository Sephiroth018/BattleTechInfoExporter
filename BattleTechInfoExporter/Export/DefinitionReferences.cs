using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Makes the <see cref="DefinitionReference" /> to a game definition that every reader shares; references that
///     need more than the definition's description are made by the reader of their section.
/// </summary>
internal static class DefinitionReferences
{
    internal static DefinitionReference ReferenceTo(BaseDescriptionDef description) =>
        new(description.Id, description.Name);

    internal static DefinitionReference ReferenceTo(FactionValue faction) =>
        new(faction.FactionDefID, FactionNames.Format(faction));
}
