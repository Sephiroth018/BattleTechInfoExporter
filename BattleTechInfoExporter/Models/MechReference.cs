using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A mech in the mech bay, by its own id and name there (<see cref="GameState.Mechs" />).</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechReference(string Id, string Name) : Reference(Id, Name);
