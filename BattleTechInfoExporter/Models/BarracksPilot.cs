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
        int? readyOnDay,
        Spirits spirits,
        ServiceRecord serviceRecord) : base(pilot)
    {
        Experience = experience;
        Injuries = injuries;
        Status = status;
        ReadyOnDay = readyOnDay;
        Spirits = spirits;
        ServiceRecord = serviceRecord;
    }

    public Experience Experience { get; }

    public int Injuries { get; }

    public PilotStatus Status { get; }

    /// <summary>
    ///     The day the pilot is ready again, on the scale of <see cref="Company.DaysPassed" />; <c>null</c> when
    ///     <see cref="PilotStatus.Ready" />.
    /// </summary>
    public int? ReadyOnDay { get; }

    public Spirits Spirits { get; }

    public ServiceRecord ServiceRecord { get; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Experience(int Unspent, int Spent);

/// <param name="EndsOnDay">
///     The day high or low spirits end, on the scale of <see cref="Company.DaysPassed" />; <c>null</c> for normal
///     spirits and when the game sets no end.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Spirits(SpiritsLevel Level, int? EndsOnDay);

/// <param name="HiredOnDay">On the scale of <see cref="Company.DaysPassed" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ServiceRecord(
    int Missions,
    int MechKills,
    int OtherKills,
    int Ejections,
    int LifetimeInjuries,
    int HiredOnDay);
