using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A mission type with the game's description of its objectives, referred to by
///     <see cref="ContractIdentity.Type" />.
/// </summary>
/// <param name="Type">The mission type; the id <c>Priority</c> stands for every priority mission.</param>
/// <param name="Description">The game's description of the type's objectives, as plain text.</param>
/// <param name="PayMultiplier">
///     Multiplies the pay of the type's contracts (see <see cref="ContractRules" />); <c>null</c> when the
///     type is <c>Priority</c>, whose pay the story sets.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ContractTypeDescription(DefinitionReference Type, string Description, float? PayMultiplier);
