using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A pilot: their id, full name without callsign, and callsign (<c>null</c> if they have none).</summary>
/// <remarks>Callsigns of generated pilots can repeat, so they're kept apart from the name.</remarks>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record PilotReference(string Id, string Name, string? Callsign);
