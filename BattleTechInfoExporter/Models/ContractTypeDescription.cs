using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A mission type with the game's description of its objectives, which <see cref="Contract.Type" /> refers to.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ContractTypeDescription(DefinitionReference Type, string Description);
