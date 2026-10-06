using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Makes the references to components, whose definitions are in the catalog. Its lookups need only the game's
///     <see cref="DataManager" />, so they also work in combat.
/// </summary>
internal static class ComponentReferences
{
    // The type mapping is static, so no SimGameState is needed.
    internal static ComponentReference ReferenceTo(
        DataManager dataManager,
        ComponentType componentType,
        string componentId) =>
        ReferenceTo(
            componentType,
            componentId,
            FindDefinition(
                dataManager,
                SimGameState.ComponentTypeToBattleTechResourceType(componentType),
                componentId));

    internal static ComponentReference ReferenceTo(BaseComponentRef component) =>
        ReferenceTo(component.ComponentDefType, component.ComponentDefID, component.Def);

    internal static ComponentReference? TryReferenceTo(
        DataManager dataManager,
        ComponentType componentType,
        string componentId) =>
        TryReferenceTo(dataManager, SimGameState.ComponentTypeToBattleTechResourceType(componentType), componentId);

    /// <summary>The reference to a component, or <c>null</c> when its definition is missing.</summary>
    /// <remarks>For lists that leave out what they can't describe, unlike the other overloads.</remarks>
    internal static ComponentReference? TryReferenceTo(
        DataManager dataManager,
        BattleTechResourceType resourceType,
        string componentId)
    {
        if (FindDefinition(dataManager, resourceType, componentId) is { } definition)
        {
            return ReferenceTo(definition.ComponentType, componentId, definition);
        }

        ModLog.Logger.LogWarning($"Left out {componentId}: no {resourceType} definition");
        return null;
    }

    // The id stands in for the name of a missing definition, which the catalog has no entry for.
    internal static ComponentReference ReferenceTo(
        ComponentType componentType,
        string componentId,
        MechComponentDef? definition)
    {
        if (definition is null)
        {
            ModLog.Logger.LogWarning($"Found no definition of {componentType} {componentId}; its id stands in");
            return new ComponentReference(componentId, componentId, componentType);
        }

        // The definition's own type, so the reference names the group that holds it.
        return new ComponentReference(componentId, NameOf(definition), definition.ComponentType);
    }

    // The mech lab shows the short UI name (MechLabItemSlotElement).
    internal static string NameOf(MechComponentDef definition) =>
        string.IsNullOrEmpty(definition.Description.UIName)
            ? definition.Description.Name
            : definition.Description.UIName;

    // DataManager.Get returns null for a missing definition.
    private static MechComponentDef? FindDefinition(
        DataManager dataManager,
        BattleTechResourceType resourceType,
        string componentId) =>
        dataManager.Get(resourceType, componentId) as MechComponentDef;
}
