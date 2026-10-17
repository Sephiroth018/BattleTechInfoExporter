namespace BattleTechInfoExporter.Models;

/// <summary>What caused an export.</summary>
internal enum ExportTrigger
{
    /// <summary>
    ///     A career is loaded, new or from a save. The game state is skipped while a finished contract's results are
    ///     still to be applied, and the catalog is rebuilt only when it's missing or stale, once the game has loaded
    ///     every vehicle, turret and design mask.
    /// </summary>
    CareerLoaded,

    /// <summary>The player saves the career outside combat.</summary>
    ManualSave,

    /// <summary>
    ///     The game saves the career on its own outside combat, e.g. once an event's popup is closed with the picked
    ///     option's results applied, or once a contract's results are applied.
    /// </summary>
    Autosave,

    /// <summary>
    ///     A contract's results are applied, salvage included, where the game doesn't save the career afterwards, which
    ///     is <see cref="Autosave" /> otherwise.
    /// </summary>
    ContractCompleted,

    /// <summary>The monthly financial report is shown, with the expenses paid.</summary>
    FinancialReportShown,

    /// <summary>
    ///     A mech lab order completes, at the end of a day or right away when it has nothing left to do; when several
    ///     work orders complete together, the last one names the export.
    /// </summary>
    MechLabOrderCompleted,

    /// <summary>A pilot's healing in the med bay completes, as <see cref="MechLabOrderCompleted" />.</summary>
    PilotHealed,

    /// <summary>An Argo upgrade's installation completes, as <see cref="MechLabOrderCompleted" />.</summary>
    ShipUpgradeCompleted,

    /// <summary>The game has generated a star system's contracts.</summary>
    ContractsGenerated,

    /// <summary>
    ///     The ship has jumped into its destination star system, at the end of that day, with the star system's
    ///     contracts; work orders completing that day export with it.
    /// </summary>
    Jumped,

    /// <summary>The ship arrives at the planet after a trip.</summary>
    Arrived,

    /// <summary>A store closes, from the main screen or the mech lab.</summary>
    StoreClosed,

    /// <summary>The store screen completes a purchase, or refuses one.</summary>
    PurchaseCompleted,

    /// <summary>The store screen completes a sale, or refuses one, once for a stack sold at once.</summary>
    SaleCompleted,

    /// <summary>
    ///     The mech bay changes: a refit, repair, readying or storing is queued, an order is cancelled, the work queue
    ///     is reordered, a mech or chassis is stored or scrapped, a stored component's repair is queued, or a mech is
    ///     renamed without a refit.
    /// </summary>
    MechBayChanged,

    /// <summary>The barracks confirm a pilot's training.</summary>
    ExperienceSpent,

    /// <summary>
    ///     The game asks where to put a new mech because every mech bay is full, e.g. after salvage completes one.
    /// </summary>
    MechPlacementShown,

    /// <summary>A mission ends, still in combat, before the salvage is chosen.</summary>
    MissionCompleted,

    /// <summary>
    ///     The after-action report has finalized the salvage: once the priority salvage is confirmed, or right away
    ///     when there is nothing to choose.
    /// </summary>
    SalvageChosen,

    /// <summary>An Argo upgrade is bought and its installation starts.</summary>
    ShipUpgradeStarted,

    /// <summary>
    ///     A phase begins, before its first unit acts: the battle's first once the lance is deployed, and every later
    ///     one, also one reached by reserving units.
    /// </summary>
    PhaseStarted,

    /// <summary>A new battle begins, when the briefing is dismissed.</summary>
    CombatStarted,

    /// <summary>A battle is loaded from a save, continuing mid-phase.</summary>
    CombatLoaded,

    /// <summary>
    ///     A unit's activation is done, its attacks resolved; out of contact, once the player's units have all moved.
    ///     Reserving a unit doesn't count.
    /// </summary>
    ActivationCompleted
}
