using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     An item collection: a weighted list the stores and the rewards draw their items from. A draw picks one entry
///     by weight; a collection drawn in full yields every entry once, as a store's own collections are.
/// </summary>
/// <param name="Components">The components a draw can yield.</param>
/// <param name="Mechs">The whole mechs a draw can yield.</param>
/// <param name="MechParts">The mech parts a draw can yield.</param>
/// <param name="Collections">The collections a draw can yield, drawn in turn.</param>
/// <param name="EmptyWeight">
///     The draw weight of the collection's empty entries, which use up a draw and yield nothing; 0 where it has
///     none.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ItemCollectionDefinition(
    string Id,
    IReadOnlyList<ComponentCollectionEntry> Components,
    IReadOnlyList<MechCollectionEntry> Mechs,
    IReadOnlyList<MechPartsCollectionEntry> MechParts,
    IReadOnlyList<NestedCollectionEntry> Collections,
    int EmptyWeight) : ICatalogEntry;

/// <summary>A component a draw from the collection can yield.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ComponentCollectionEntry(ComponentReference Component, int? Count, int Weight)
    : ComponentEntry(Component), IItemCollectionItem;

/// <summary>A whole mech a draw from the collection can yield.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechCollectionEntry(DefinitionReference Mech, int? Count, int Weight)
    : MechEntry(Mech), IItemCollectionItem;

/// <summary>Mech parts a draw from the collection can yield.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MechPartsCollectionEntry(DefinitionReference Mech, int? Count, int Weight)
    : MechPartsEntry(Mech), IItemCollectionItem;

/// <summary>Another collection a draw from the collection can yield, which is then drawn from in turn.</summary>
/// <param name="Collection">The collection drawn from, by its id in <see cref="Catalog.ItemCollectionDefinitions" />.</param>
/// <param name="Draws">
///     The draws from the collection, each by weight and with the same entry possible again; <c>null</c>: every
///     entry once.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record NestedCollectionEntry(string Collection, int? Draws, int Weight) : IItemCollectionEntry;
