using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A team's side, seen from the player: a unit's or a building's.</summary>
/// <param name="Faction">The faction of the team.</param>
/// <param name="Allegiance">Whose side the team is on, seen from the player.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record TeamSide(DefinitionReference Faction, UnitAllegiance Allegiance);
