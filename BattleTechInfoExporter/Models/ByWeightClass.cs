using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A value that depends on the unit's weight class.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ByWeightClass<T>(T Light, T Medium, T Heavy, T Assault);
