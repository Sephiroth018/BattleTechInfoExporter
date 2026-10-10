using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The trip from the current star system, as the starmap plans and shows it.</summary>
/// <param name="Days">The days the trip takes; 0 for the current star system.</param>
/// <param name="Cost">The C-Bills the trip costs; 0 for the current star system.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Route(int Days, int Cost);
