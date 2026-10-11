using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A whole mech in one of its states (for sale, in an item collection), as opposed to parts of it or a mech in
///     the mech bay; its definition is in <see cref="Catalog.MechDefinitions" />.
/// </summary>
/// <param name="Mech">The mech, described in <see cref="Catalog.MechDefinitions" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record MechEntry(DefinitionReference Mech);
