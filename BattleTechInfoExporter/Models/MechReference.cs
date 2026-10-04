using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A mech in the mech bay: its own id, as in <see cref="Mech.Id" />, and its name with the variant.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechReference(string Id, string Name);
