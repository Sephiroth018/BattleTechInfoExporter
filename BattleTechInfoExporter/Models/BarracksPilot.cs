using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A pilot in the barracks: the commander or a roster member.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record BarracksPilot : Pilot
{
    internal BarracksPilot(
        Pilot pilot,
        Experience experience,
        int injuries,
        PilotStatus status,
        int daysUntilReady,
        Spirits spirits,
        ServiceRecord serviceRecord) : base(pilot)
    {
        Experience = experience;
        Injuries = injuries;
        Status = status;
        DaysUntilReady = daysUntilReady;
        Spirits = spirits;
        ServiceRecord = serviceRecord;
    }

    public Experience Experience { get; }

    public int Injuries { get; }

    public PilotStatus Status { get; }

    /// <summary>Zero when the pilot is <see cref="PilotStatus.Ready" />.</summary>
    public int DaysUntilReady { get; }

    public Spirits Spirits { get; }

    public ServiceRecord ServiceRecord { get; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Experience(int Unspent, int Spent);

/// <param name="DaysRemaining">Until high or low spirits end; <c>null</c> for normal spirits, which don't.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Spirits(SpiritsLevel Level, int? DaysRemaining);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ServiceRecord(
    int Missions,
    int MechKills,
    int OtherKills,
    int Ejections,
    int LifetimeInjuries,
    int DaysOnCrew);
