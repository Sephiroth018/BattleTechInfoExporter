using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>Something the export refers to by its id instead of repeating it, with the name the UI shows.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record Reference(string Id, string Name);
