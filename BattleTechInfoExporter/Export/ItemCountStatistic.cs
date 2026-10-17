using BattleTech;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     The name of the company statistic that counts a stored item, as
///     <see cref="SimGameState.GetItemStatID(string, string)" /> makes it: <c>Item.&lt;type&gt;.&lt;id&gt;</c>, with a
///     <c>.DAMAGED</c> suffix for the damaged copies (<see cref="SimGameState.AddItemStat(string, string, bool)" />).
/// </summary>
/// <param name="Type">The item's type, e.g. <c>WeaponDef</c>, <c>MechDef</c> or <c>MECHPART</c>.</param>
/// <param name="Id">The item's definition id.</param>
/// <param name="IsDamaged">Whether the statistic counts the damaged copies.</param>
internal sealed record ItemCountStatistic(string Type, string Id, bool IsDamaged)
{
    private const string DamagedSuffix = "DAMAGED";

    /// <summary>The parts of the name, or <c>null</c> for a statistic that counts no stored item.</summary>
    /// <remarks>Split as <see cref="SimGameState.GetAllInventoryItemDefs" /> splits it.</remarks>
    internal static ItemCountStatistic? TryParse(string statisticName)
    {
        var parts = statisticName.Split('.');
        return parts.Length >= 3 && parts[0] == SimGameState.COMPANYSTAT_ITEMCOUNT
            ? new ItemCountStatistic(parts[1], parts[2], parts.Length > 3 && parts[3] == DamagedSuffix)
            : null;
    }
}
