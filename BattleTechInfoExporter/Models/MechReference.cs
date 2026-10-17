using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A mech in the mech bay, by its own id and name there (<see cref="GameState.Mechs" />).</summary>
/// <param name="Id">The game's own id of the mech in the mech bay.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechReference(string Id, string Name) : Reference(Id, Name);
