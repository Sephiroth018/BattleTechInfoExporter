using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A component in one of its states (stored, for sale, mounted, damaged, ...); its definition is in
///     <see cref="Catalog.ComponentDefinitions" />. Each state derives from it with the fields of its own.
/// </summary>
/// <param name="Component">The component, by the id its definition is keyed by.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record ComponentEntry(ComponentReference Component);
