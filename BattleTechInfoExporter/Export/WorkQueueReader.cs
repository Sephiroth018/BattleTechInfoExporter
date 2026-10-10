using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using UnityEngine;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the work queue from the game's career state, as the timeline shows it.</summary>
internal static class WorkQueueReader
{
    // Adds the entries in the order of TaskTimelineWidget.RegenerateEntries; the stable sort keeps it among entries
    // finishing on the same day, which the timeline's unstable sort may show in any order.
    internal static List<WorkQueueEntry> ReadWorkQueue(
        SimGameState simGame,
        IReadOnlyDictionary<WorkOrderEntry, int> mechLabFinishingDays)
    {
        var entries = simGame.MechLabQueue
            // SimGameState.UpdateMechLabWorkQueue relies on the same.
            .Cast<WorkOrderEntry_MechLab>()
            .Select(order => ReadMechLabEntry(simGame, order, mechLabFinishingDays[order]))
            // The med bay holds heal orders only (SimGameState.RefreshInjuries). The pilot's timeout comes from their
            // heal order, counted as the timeline does.
            .Concat(simGame.MedBayQueue.SubEntries
                .OfType<WorkOrderEntry_MedBayHeal>()
                .Select(order => new WorkQueueEntry(
                    order.Type,
                    PilotReader.ReadReadyOnDay(simGame, order.Pilot),
                    PilotReader.ReferenceTo(order.Pilot))))
            .ToList();

        if (simGame.TravelOrder is { } travelOrder)
        {
            entries.Add(ReadTravelEntry(simGame, travelOrder));
        }

        if (simGame.FinancialReportNotification is { } report)
        {
            entries.Add(new WorkQueueEntry(report.Type, FinancialReportReader.ReadDueOnDay(simGame), null));
        }

        if (simGame.CurrentUpgradeEntry is { } upgradeOrder)
        {
            entries.Add(ReadShipUpgradeEntry(simGame, upgradeOrder));
        }

        return entries.OrderBy(entry => entry.FinishesOnDay).ToList();
    }

    // Mirrors TaskTimelineWidget.RefreshEntries: the mech techs work on the first order only, so each order waits
    // for the ones before it; an order without mech techs counts on its own, and a paid one shows as done.
    internal static Dictionary<WorkOrderEntry, int> ReadMechLabFinishingDays(SimGameState simGame)
    {
        var finishingDays = new Dictionary<WorkOrderEntry, int>();
        var cumulativeDays = 0;
        foreach (var order in simGame.MechLabQueue)
        {
            var days = DaysUntilFinished(order, simGame.MechTechSkill);
            if (!order.IsCostPaid() && simGame.WorkOrderIsMechTech(order.Type))
            {
                cumulativeDays += days;
                days = cumulativeDays;
            }

            finishingDays[order] = simGame.DaysPassed + days;
        }

        return finishingDays;
    }

    /// <summary>The day the ship arrives at the end of its travel order.</summary>
    /// <remarks>
    ///     The travel order keeps the legs as internal sub-entries, so its remaining cost is the whole trip, as the
    ///     timeline shows it; the timeline counts travel one cost per day (TaskManagementElement.UpdateTaskInfo).
    /// </remarks>
    internal static int ReadArrivalDay(SimGameState simGame, WorkOrderEntry_TravelGeneric travelOrder) =>
        simGame.DaysPassed + DaysUntilFinished(travelOrder, 1);

    /// <summary>
    ///     The day the leg under way ends: the travel order's legs are its sub-entries (to the jump point, each jump, to
    ///     the planet; Starmap.OnPathfindingComplete), paid in order one cost per day (WorkOrderEntry.PayCost).
    /// </summary>
    internal static int ReadLegEndDay(SimGameState simGame, WorkOrderEntry_TravelGeneric travelOrder)
    {
        var remainingCost = 0;
        foreach (var leg in travelOrder.SubEntries)
        {
            remainingCost += leg.GetRemainingCost();
            if (!leg.IsCostPaid())
            {
                break;
            }
        }

        return simGame.DaysPassed + DaysFor(remainingCost, 1);
    }

    // Mirrors TaskManagementElement.UpdateItem for a single entry: a paid entry is done. The daily progress is the
    // entry type's, e.g. SimGameState.DailyUpgradeValue for a ship upgrade.
    private static int DaysUntilFinished(WorkOrderEntry entry, int dailyProgress) =>
        entry.IsCostPaid() ? 0 : DaysFor(entry.GetRemainingCost(), dailyProgress);

    /// <summary>The days work of this cost takes, at least one, as the timeline counts them.</summary>
    internal static int DaysFor(int cost, int dailyProgress) =>
        Mathf.Max(1, Mathf.CeilToInt((float)cost / dailyProgress));

    // The timeline names the mech by SimGameState.GetMechByID, which also finds readying mechs.
    private static WorkQueueEntry ReadMechLabEntry(
        SimGameState simGame,
        WorkOrderEntry_MechLab order,
        int finishesOnDay)
    {
        var mech = simGame.GetMechByID(order.MechID);
        if (mech is null)
        {
            ModLog.Logger.LogWarning($"Work order {order.ID} names no mech of the company: {order.MechID}");
        }

        return new WorkQueueEntry(
            order.Type,
            finishesOnDay,
            mech is null ? null : MechReader.ReferenceToBayMech(mech));
    }

    private static WorkQueueEntry ReadTravelEntry(SimGameState simGame, WorkOrderEntry_TravelGeneric travelOrder)
    {
        var destination = simGame.Starmap?.Destination?.System;
        if (destination is null)
        {
            ModLog.Logger.LogWarning($"Travel order {travelOrder.ID} has no destination");
        }

        return new WorkQueueEntry(
            travelOrder.Type,
            ReadArrivalDay(simGame, travelOrder),
            destination is null ? null : DefinitionReferences.ReferenceTo(destination.Def.Description));
    }

    private static WorkQueueEntry ReadShipUpgradeEntry(
        SimGameState simGame,
        WorkOrderEntry_ArgoUpgradeGeneric upgradeOrder)
    {
        if (!simGame.DataManager.ShipUpgradeDefs.TryGet(upgradeOrder.upgradeID, out var upgrade))
        {
            ModLog.Logger.LogWarning(
                $"Left out {upgradeOrder.upgradeID}: no {nameof(BattleTechResourceType.ShipModuleUpgrade)} definition");
        }

        return new WorkQueueEntry(
            upgradeOrder.Type,
            simGame.DaysPassed + DaysUntilFinished(upgradeOrder, simGame.DailyUpgradeValue),
            upgrade is null ? null : DefinitionReferences.ReferenceTo(upgrade.Description));
    }
}
