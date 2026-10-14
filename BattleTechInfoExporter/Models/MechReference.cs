using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A mech in the mech bay: its own id and name, as in <see cref="Mech.Id" /> and <see cref="Mech.Name" />.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechReference(string Id, string Name) : Reference(Id, Name);
