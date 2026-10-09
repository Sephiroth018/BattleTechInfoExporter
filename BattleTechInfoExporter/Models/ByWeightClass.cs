using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Light">The value for light units.</param>
/// <param name="Medium">The value for medium units.</param>
/// <param name="Heavy">The value for heavy units.</param>
/// <param name="Assault">The value for assault units.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ByWeightClass<T>(T Light, T Medium, T Heavy, T Assault);
