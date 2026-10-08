using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     Parts of a mech, stored or for sale; with <see cref="Rules.MechPartsPerMech" /> of them in storage they become
///     that mech.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record MechPartsEntry(DefinitionReference Mech);
