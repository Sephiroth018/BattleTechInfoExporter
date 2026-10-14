using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace BattleTechInfoExporter.Export;

/// <summary>Tables keyed by weapon category, by the name the catalog's weapons give their category.</summary>
internal static class WeaponCategories
{
    /// <summary>A value per weapon category the game knows, keyed by its name.</summary>
    internal static SortedDictionary<string, TValue> ByName<TValue>(Func<WeaponCategoryValue, TValue> readValue) =>
        new(
            WeaponCategoryEnumeration.WeaponCategoryList
                .Where(category => !category.Is_NotSet)
                .ToDictionary(category => category.FriendlyName, readValue),
            StringComparer.Ordinal);
}
