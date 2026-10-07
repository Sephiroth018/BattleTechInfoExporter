using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using StarSystem = BattleTech.StarSystem;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the game state file's models from the game's career state.</summary>
internal static class GameStateReader
{
    internal static GameState Read(SimGameState simGame, ExportTrigger trigger)
    {
        var travelInProgress = ReadTravelInProgress(simGame);
        var mechLabFinishingDays = WorkQueueReader.ReadMechLabFinishingDays(simGame);
        return new GameState(
            ModAssembly.Version,
            trigger,
            ReadCompany(simGame),
            WorkQueueReader.ReadWorkQueue(simGame, mechLabFinishingDays),
            ShipReader.ReadShip(simGame),
            PilotReader.ReadPilots(simGame),
            MechReader.ReadMechs(simGame, mechLabFinishingDays),
            MechReader.ReadMechsAwaitingPlacement(simGame),
            LanceReader.ReadLastLance(simGame),
            StorageReader.ReadStorage(simGame),
            StoreReader.ReadStores(simGame),
            PilotReader.ReadHiringHall(simGame),
            ContractReader.ReadActiveContract(simGame, travelInProgress),
            ContractReader.ReadContracts(simGame),
            ReadPosition(simGame, travelInProgress));
    }

    private static Company ReadCompany(SimGameState simGame) =>
        new(
            simGame.CompanyName,
            simGame.CurDropship,
            simGame.DaysPassed,
            simGame.CurrentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            new Morale(simGame.Morale, simGame.GetCurrentMoraleLevelDescriptor()),
            FinancialReportReader.ReadFinancialReport(simGame),
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
            simGame.GetMaxMechWarriors());

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
        (StarSystem Destination, int ArrivesOnDay)? travelInProgress) =>
        new(
            DefinitionReferences.ReferenceTo(simGame.CurSystem.Def.Description),
            simGame.TravelState,
            travelInProgress is ({ } destination, var arrivesOnDay)
                ? new Travel(DefinitionReferences.ReferenceTo(destination.Def.Description), arrivesOnDay)
                : null);

    // TravelTime only counts the current leg (e.g. to the jump point); the travel order counts the whole trip.
    private static (StarSystem Destination, int ArrivesOnDay)? ReadTravelInProgress(SimGameState simGame)
    {
        var destination = simGame.Starmap?.Destination?.System;
        var travelOrder = simGame.TravelOrder;
        return simGame.TravelState == SimGameTravelStatus.IN_SYSTEM || destination is null || travelOrder is null
            ? null
            : (destination, WorkQueueReader.ReadArrivalDay(simGame, travelOrder));
    }
}
