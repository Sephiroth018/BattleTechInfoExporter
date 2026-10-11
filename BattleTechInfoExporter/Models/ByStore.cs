using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A value per store of a star system: its system store, faction store and black market.</summary>
/// <param name="System">The value for the system store.</param>
/// <param name="Faction">The value for the faction store.</param>
/// <param name="BlackMarket">The value for the black market.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ByStore<T>(T System, T Faction, T BlackMarket);
