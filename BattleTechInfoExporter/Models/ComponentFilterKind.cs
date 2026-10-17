namespace BattleTechInfoExporter.Models;

/// <summary>The property of a component an effect's filter compares, in the order the game checks them.</summary>
internal enum ComponentFilterKind
{
    /// <summary>Compares a weapon's sub type.</summary>
    WeaponSubType,

    /// <summary>Compares a weapon's type.</summary>
    WeaponType,

    /// <summary>Compares a weapon's category, e.g. energy or missile.</summary>
    WeaponCategory,

    /// <summary>Compares an ammunition box's ammunition category.</summary>
    AmmoCategory
}
