using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Travel(DefinitionReference Destination, DefinitionReference DestinationOwner, int DaysLeft);
