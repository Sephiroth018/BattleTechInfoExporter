using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ByWeightClass<T>(T Light, T Medium, T Heavy, T Assault);
