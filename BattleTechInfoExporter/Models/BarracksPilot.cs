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

    /// <summary>The pilot's experience points, spent and unspent.</summary>
    public Experience Experience { get; }

    /// <summary>The injuries the pilot has, out of <see cref="Pilot.Health" />; 0 when uninjured.</summary>
    public int Injuries { get; }

    /// <summary>Whether the pilot is ready, injured or out of action because of an event.</summary>
    public PilotStatus Status { get; }

    /// <summary>
    ///     The day the pilot is ready again, on the scale of <see cref="Company.DaysPassed" />; <c>null</c> when
    ///     <see cref="PilotStatus.Ready" />.
    /// </summary>
    public int? ReadyOnDay { get; }

    /// <summary>The pilot's spirits, high, normal or low.</summary>
    public Spirits Spirits { get; }

    /// <summary>The pilot's missions, kills and injuries so far, and the day of hire.</summary>
    public ServiceRecord ServiceRecord { get; }
}

/// <param name="Unspent">The experience points available to spend on skills.</param>
/// <param name="Spent">The experience points already spent on skills.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Experience(int Unspent, int Spent);

/// <param name="Level">The spirits level, whose effects are in <see cref="Rules.SpiritsLevels" />.</param>
/// <param name="EndsOnDay">
///     The day high or low spirits end, on the scale of <see cref="Company.DaysPassed" />; <c>null</c> for normal
///     spirits and when the game sets no end.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Spirits(SpiritsLevel Level, int? EndsOnDay);

/// <param name="Missions">The missions the pilot has fought.</param>
/// <param name="MechKills">The mechs the pilot has destroyed.</param>
/// <param name="OtherKills">The other units the pilot has destroyed: vehicles and turrets.</param>
/// <param name="Ejections">The missions the pilot ejected in.</param>
/// <param name="LifetimeInjuries">The injuries the pilot has taken in missions, healed ones included.</param>
/// <param name="HiredOnDay">On the scale of <see cref="Company.DaysPassed" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ServiceRecord(
    int Missions,
    int MechKills,
    int OtherKills,
    int Ejections,
    int LifetimeInjuries,
    int HiredOnDay);
