using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A pilot for hire in the current system's hiring hall.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HiringHallPilot : Pilot
{
    internal HiringHallPilot(Pilot pilot, int hireCost, int salary, bool canHire) : base(pilot)
    {
        HireCost = hireCost;
        Salary = salary;
        CanHire = canHire;
    }

    /// <summary>The one-time C-Bills to hire the pilot.</summary>
    public int HireCost { get; }

    /// <summary>What the pilot would add to the expenses of each report, as a crew member's line does.</summary>
    public int Salary { get; }

    /// <summary>
    ///     Whether the company's Mercenary Review Board rating and morale allow hiring the pilot; full barracks
    ///     (<see cref="Company.MaxPilots" />), travel and funds can block it as well.
    /// </summary>
    public bool CanHire { get; }
}
