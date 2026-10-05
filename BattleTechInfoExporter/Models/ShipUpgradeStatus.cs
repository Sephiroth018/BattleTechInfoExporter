namespace BattleTechInfoExporter.Models;

/// <summary>Where a ship upgrade stands, as the Argo's engineering screen shows it.</summary>
internal enum ShipUpgradeStatus
{
    Installed,

    /// <summary>Bought and being installed; it finishes as the work queue shows.</summary>
    Installing,

    /// <summary>
    ///     Its required upgrades are all installed, so it can be bought, unless another upgrade is being installed
    ///     or funds are short.
    /// </summary>
    Available,

    /// <summary>Not all of its required upgrades are installed yet, but each is installed, installing or available.</summary>
    Locked
}
