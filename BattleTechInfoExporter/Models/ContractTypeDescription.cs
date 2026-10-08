using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A mission type with the game's description of its objectives, which <see cref="ContractIdentity.Type" />
///     refers to.
/// </summary>
/// <param name="PayMultiplier">
///     Multiplies the pay of the type's contracts (see <see cref="ContractRules" />); <c>null</c> for priority
///     missions, whose pay the story sets.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ContractTypeDescription(DefinitionReference Type, string Description, float? PayMultiplier);
