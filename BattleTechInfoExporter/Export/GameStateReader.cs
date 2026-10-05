using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the game state file's models from the game's career state.</summary>
internal static class GameStateReader
{
    internal static GameState Read(SimGameState simGame, ExportTrigger trigger)
    {
        var componentReferences = new ComponentReferences(simGame.DataManager);
        var travelInProgress = ReadTravelInProgress(simGame);
        return new GameState(
            ModAssembly.Version,
            trigger,
            ReadCompany(simGame),
            PilotReader.ReadPilots(simGame),
            MechReader.ReadMechs(simGame, componentReferences),
            LanceReader.ReadLastLance(simGame),
            StorageReader.ReadStorage(simGame, componentReferences),
            StoreReader.ReadStores(simGame, componentReferences),
            PilotReader.ReadHiringHall(simGame),
            ContractReader.ReadActiveContract(simGame, travelInProgress),
            ContractReader.ReadContracts(simGame),
            // After every section that references components.
            componentReferences.Definitions,
            ReadPosition(simGame, travelInProgress));
    }

    private static Company ReadCompany(SimGameState simGame) =>
        new(
            simGame.CompanyName,
            simGame.CurDropship,
            simGame.DaysPassed,
            simGame.CurrentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            new Morale(simGame.Morale, simGame.GetCurrentMoraleLevelDescriptor()),
            FinancesReader.ReadFinances(simGame),
            new MercenaryReviewBoard(
                simGame.GetRawReputation(FactionEnumeration.GetMercenaryReviewBoardFactionValue()),
                simGame.GetCurrentMRBLevel()),
            // The game models the Mercenary Review Board as a faction, but it isn't one; it has its own object.
            FactionEnumeration.FactionList
                .Where(faction => faction.DoesGainReputation && !faction.IsMercenaryReviewBoard)
                .Select(faction => ReadReputation(simGame, faction))
                .ToList(),
            simGame.MechTechSkill,
            simGame.MedTechSkill,
            simGame.GetMaxMechWarriors());

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
        (StarSystem Destination, int DaysLeft)? travelInProgress) =>
        new(
            DefinitionReferences.ReferenceTo(simGame.CurSystem.Def.Description),
            DefinitionReferences.ReferenceTo(simGame.CurSystem.OwnerValue),
            SystemTags.ReadVisibleTags(simGame.CurSystem),
            // StarSystemDef.SupportedBiomes limits the maps of the system's contracts (SimGameState.GetSinglePlayerProceduralPlayableMaps).
            simGame.CurSystem.Def.SupportedBiomes
                .Select(biome => DefinitionReferences.ReferenceTo(simGame.DataManager, biome))
                .ToList(),
            simGame.TravelState,
            travelInProgress is ({ } destination, var daysLeft)
                ? new Travel(
                    DefinitionReferences.ReferenceTo(destination.Def.Description),
                    DefinitionReferences.ReferenceTo(destination.OwnerValue),
                    daysLeft)
                : null);

    // TravelTime only counts the current leg (e.g. to the jump point). The travel order keeps the legs as
    // internal sub-entries, so its remaining cost is the whole trip, the single entry the queue shows.
    private static (StarSystem Destination, int DaysLeft)? ReadTravelInProgress(SimGameState simGame)
    {
        var destination = simGame.Starmap?.Destination?.System;
        var travelOrder = simGame.TravelOrder;
        return simGame.TravelState == SimGameTravelStatus.IN_SYSTEM || destination is null || travelOrder is null
            ? null
            : (destination, travelOrder.GetRemainingCost());
    }
}
