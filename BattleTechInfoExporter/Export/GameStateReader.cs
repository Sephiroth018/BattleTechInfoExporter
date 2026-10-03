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
            new MercenaryReviewBoard(
                simGame.GetRawReputation(FactionEnumeration.GetMercenaryReviewBoardFactionValue()),
                simGame.GetCurrentMRBLevel()),
            // The game models the Mercenary Review Board as a faction, but it isn't one; it has its own object.
            FactionEnumeration.FactionList
                .Where(faction => faction.DoesGainReputation && !faction.IsMercenaryReviewBoard)
                .Select(faction => ReadReputation(simGame, faction))
                .ToList());

    private static FactionReputation ReadReputation(SimGameState simGame, FactionValue faction) =>
        new(
            ReferenceTo(faction),
            simGame.GetRawReputation(faction),
            simGame.GetReputation(faction),
            simGame.IsFactionAlly(faction),
            simGame.IsFactionEnemy(faction),
            simGame.displayedFactions.Contains(faction.Name));

    private static Position ReadPosition(SimGameState simGame) =>
        new(
            new DefinitionReference(simGame.CurSystem.Def.Description.Id, simGame.CurSystem.Def.Description.Name),
            ReferenceTo(simGame.CurSystem.OwnerValue),
            simGame.TravelState);

    // Some factions (e.g. placeholders) have no FactionDef, and with it no display name.
    private static DefinitionReference ReferenceTo(FactionValue faction) =>
        new(faction.FactionDefID, CapitalizeFirstLetter(faction.FactionDef?.Name));

    // Faction names are written for use mid-sentence ("the Federated Suns"); the UI only hides that
    // by showing them in uppercase.
    private static string? CapitalizeFirstLetter(string? text) =>
        text is null or "" ? text : char.ToUpperInvariant(text[0]) + text.Substring(1);
}
