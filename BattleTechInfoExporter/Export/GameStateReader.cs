using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using Starmap = BattleTechInfoExporter.Models.Starmap;
using StarSystem = BattleTech.StarSystem;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the game state file's models from the game's career state.</summary>
internal static class GameStateReader
{
    // The star systems the game state shows in full are taken from the star systems file, as they are there.
    internal static GameState Read(SimGameState simGame, Starmap starmap)
    {
        var travelInProgress = ReadTravelInProgress(simGame);
        var mechLabFinishingDays = WorkQueueReader.ReadMechLabFinishingDays(simGame);
        var comparedStatistics = EventStatisticReader.ReadComparedStatisticNames(simGame.DataManager);
        return new GameState(
            ReadCompany(simGame, comparedStatistics.Company),
            FinancialReportReader.Read(simGame),
            WorkQueueReader.ReadWorkQueue(simGame, mechLabFinishingDays),
            ShipReader.ReadShip(simGame),
            PilotReader.ReadPilots(simGame, comparedStatistics.Pilot),
            MechReader.ReadMechs(simGame, mechLabFinishingDays),
            MechReader.ReadMechsAwaitingPlacement(simGame),
            LanceReader.ReadLastLance(simGame),
            StorageReader.ReadStorage(simGame),
            StoreReader.ReadStores(simGame),
            PilotReader.ReadHiringHall(simGame),
            ContractReader.ReadActiveContract(simGame, starmap, travelInProgress),
            ContractReader.ReadContracts(simGame, starmap),
            ReadPosition(simGame, starmap, travelInProgress),
            EventStateReader.ReadEventState(simGame));
    }

    private static Company ReadCompany(SimGameState simGame, IReadOnlyList<string> comparedStatistics) =>
        new(
            simGame.CompanyName,
            simGame.CurDropship,
            simGame.DaysPassed,
            simGame.CurrentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            new Morale(simGame.Morale, simGame.GetCurrentMoraleLevelDescriptor()),
            simGame.Funds,
            new MercenaryReviewBoard(
                simGame.GetRawReputation(FactionEnumeration.GetMercenaryReviewBoardFactionValue()),
                simGame.GetCurrentMRBLevel()),
            // The game models the Mercenary Review Board as a faction, but it isn't one; it has its own object.
            FactionEnumeration.FactionList
                .Where(faction => faction.DoesGainReputation && !faction.IsMercenaryReviewBoard)
                .Select(faction => ReadReputation(simGame, faction))
                .ToList(),
            simGame.MechTechSkill,
            ReadTemporaryChanges(simGame, SimGameState.COMPANYSTAT_MECHTECH),
            simGame.MedTechSkill,
            ReadTemporaryChanges(simGame, SimGameState.COMPANYSTAT_MEDTECH),
            simGame.GetMaxMechWarriors(),
            simGame.CompanyTags.ToList(),
            EventStatisticReader.ReadStatistics(simGame.CompanyStats, comparedStatistics));

    // SimGameState.UpdateTempResults reverts a temporary result as the day after its last day starts, adding the
    // negated amount of every stat it doesn't set.
    private static List<TemporaryChange> ReadTemporaryChanges(SimGameState simGame, string companyStatName) =>
        simGame.TemporaryResultTracker
            .Where(result => result.Scope == EventScope.Company && result.Stats is not null)
            .SelectMany(result => result.Stats
                .Where(stat => stat.name == companyStatName && !stat.set)
                .Select(stat => new TemporaryChange(
                    stat.ToInt(),
                    // The days left as SimGameState.GetTemporaryTagLength counts them, as for a pilot's spirits.
                    simGame.DaysPassed + result.ResultDuration - result.DaysElapsed)))
            .OrderBy(change => change.EndsOnDay)
            .ToList();

    private static FactionReputation ReadReputation(SimGameState simGame, FactionValue faction) =>
        new(
            DefinitionReferences.ReferenceTo(faction),
            simGame.GetRawReputation(faction),
            simGame.GetReputation(faction),
            simGame.IsFactionAlly(faction),
            simGame.IsFactionEnemy(faction),
            simGame.displayedFactions.Contains(faction.Name));

    private static Position ReadPosition(
        SimGameState simGame,
        Starmap starmap,
        (StarSystem Destination, int ArrivesOnDay, int LegEndsOnDay)? travelInProgress) =>
        new(
            starmap.StarSystems[simGame.CurSystem.Def.Description.Id],
            simGame.TravelState,
            travelInProgress is ({ } destination, var arrivesOnDay, var legEndsOnDay)
                ? new Travel(
                    starmap.StarSystems[destination.Def.Description.Id],
                    arrivesOnDay,
                    simGame.CurSystem.JumpDistance,
                    legEndsOnDay)
                : null);

    // TravelTime only counts the current leg (e.g. to the jump point); the travel order counts the whole trip.
    private static (StarSystem Destination, int ArrivesOnDay, int LegEndsOnDay)? ReadTravelInProgress(
        SimGameState simGame)
    {
        var destination = simGame.Starmap?.Destination?.System;
        var travelOrder = simGame.TravelOrder;
        return simGame.TravelState == SimGameTravelStatus.IN_SYSTEM || destination is null || travelOrder is null
            ? null
            : (destination,
                WorkQueueReader.ReadArrivalDay(simGame, travelOrder),
                WorkQueueReader.ReadLegEndDay(simGame, travelOrder));
    }
}
