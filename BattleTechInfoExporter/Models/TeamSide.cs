using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A team's side: a unit's or a building's.</summary>
/// <param name="Faction">The faction of the team.</param>
/// <param name="Allegiance">Whose side the team is on.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record TeamSide(DefinitionReference Faction, UnitAllegiance Allegiance);
