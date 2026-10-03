using System;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the export's model from the game's career state.</summary>
internal static class GameStateReader
{
    internal static GameState Read(SimGameState simGame, ExportTrigger trigger) =>
        new(ModAssembly.Version, DateTimeOffset.Now, trigger, ReadCompany(simGame), ReadPosition(simGame));

    private static Company ReadCompany(SimGameState simGame) =>
        new(
            simGame.CompanyName,
            simGame.Funds,
            simGame.GetExpenditures(),
            simGame.DaysPassed,
            simGame.CurrentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            simGame.GetCurrentMRBLevel(),
            FactionEnumeration.FactionList
                .Where(faction => faction.DoesGainReputation)
                .Select(faction => ReadReputation(simGame, faction))
                .ToList());

    private static FactionReputation ReadReputation(SimGameState simGame, FactionValue faction) =>
        new(
            ReferenceTo(faction),
            simGame.GetRawReputation(faction),
            simGame.GetReputation(faction),
            simGame.IsFactionAlly(faction),
            simGame.IsFactionEnemy(faction));

    private static Position ReadPosition(SimGameState simGame) =>
        new(
            new DefinitionReference(simGame.CurSystem.Def.Description.Id, simGame.CurSystem.Def.Description.Name),
            ReferenceTo(simGame.CurSystem.OwnerValue),
            simGame.TravelState);

    // Some factions (e.g. placeholders) have no FactionDef, and with it no display name.
    private static DefinitionReference ReferenceTo(FactionValue faction) =>
        new(faction.FactionDefID, faction.FactionDef?.Name);
}
