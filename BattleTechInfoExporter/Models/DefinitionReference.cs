using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A game definition: its id from the game's data files and the name shown in the UI.</summary>
/// <param name="Id">
///     The definition's id from the game's data files, or its name in the game's enumeration where it has no data
///     file (e.g. the player's faction).
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record DefinitionReference(string Id, string Name) : Reference(Id, Name);
