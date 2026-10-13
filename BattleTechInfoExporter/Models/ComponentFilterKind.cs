namespace BattleTechInfoExporter.Models;

/// <summary>The property of a component an effect's filter compares, in the order the game checks them.</summary>
internal enum ComponentFilterKind
{
    WeaponSubType,
    WeaponType,
    WeaponCategory,
    AmmoCategory
}
