using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A game definition: its id from the game's data files and the name shown in the UI.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record DefinitionReference(string Id, string Name);
