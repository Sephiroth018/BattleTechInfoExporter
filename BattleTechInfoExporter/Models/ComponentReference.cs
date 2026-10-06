using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     A component's definition, like <see cref="DefinitionReference" />, with the type that tells which group of
///     <see cref="Catalog.ComponentDefinitions" /> holds it.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ComponentReference(string Id, string Name, ComponentType Type);
