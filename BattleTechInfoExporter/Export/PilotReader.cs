using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the pilots of the barracks from the game's career state.</summary>
internal static class PilotReader
{
    // The commander is kept apart from the roster; the barracks lists them first (SGBarracksWidget.Reset).
    internal static List<BarracksPilot> ReadPilots(SimGameState simGame) =>
        new[] { simGame.Commander }
            .Concat(simGame.PilotRoster)
            .Select(pilot => ReadPilot(simGame, pilot))
            .ToList();

    internal static PilotReference ReferenceTo(HumanDescriptionDef pilot) =>
        new(pilot.Id, FullName(pilot), pilot.Callsign);

    private static BarracksPilot ReadPilot(SimGameState simGame, Pilot pilot)
    {
        var definition = pilot.pilotDef;
        var description = definition.Description;
        return new BarracksPilot(
            description.Id,
            FullName(description),
            description.Callsign,
            ReadPilotType(simGame, pilot),
            description.Age,
            description.Gender,
            simGame.GetPilotFullExpertise(pilot),
            new Skills(pilot.Gunnery, pilot.Piloting, pilot.Guts, pilot.Tactics),
            new Experience(pilot.UnspentXP, pilot.SpentXP),
            SimGameState.GetPrimaryPilotAbilities(definition)
                .Select(ability => GameStateReader.ReferenceTo(ability.Description))
                .ToList(),
            pilot.Health,
            pilot.Injuries,
            ReadPilotStatus(pilot),
            simGame.GetPilotTimeoutTimeRemaining(pilot),
            ReadSpirits(simGame, pilot),
            new ServiceRecord(
                definition.MissionsPiloted,
                definition.MechKills,
                definition.OtherKills,
                definition.MissionsEjected,
                definition.LifetimeInjuries,
                simGame.DaysPassed - definition.DateOfHire));
    }

    // Mirrors SimGameState.GetPilotTypeColor.
    private static PilotType ReadPilotType(SimGameState simGame, Pilot pilot) =>
        pilot == simGame.Commander ? PilotType.Commander
        : pilot.pilotDef.IsVanguard ? PilotType.Vanguard
        : pilot.pilotDef.IsRonin ? PilotType.Ronin
        : PilotType.Regular;

    // Mirrors SGBarracksDossierPanel.SetPilot: injuries take precedence over an event timeout.
    private static PilotStatus ReadPilotStatus(Pilot pilot) =>
        pilot.Injuries > 0 ? PilotStatus.Injured
        : pilot.pilotDef.TimeoutRemaining > 0 ? PilotStatus.Unavailable
        : PilotStatus.Ready;

    private static Spirits ReadSpirits(SimGameState simGame, Pilot pilot) =>
        pilot switch
        {
            { HasHighMorale: true } => new Spirits(
                SpiritsLevel.High,
                simGame.GetTemporaryTagLength(pilot, Pilot.PILOTDEFTAG_HIGH_MORALE)),
            { HasLowMorale: true } => new Spirits(
                SpiritsLevel.Low,
                simGame.GetTemporaryTagLength(pilot, Pilot.PILOTDEFTAG_LOW_MORALE)),
            _ => new Spirits(SpiritsLevel.Normal, null)
        };

    private static string FullName(HumanDescriptionDef pilot) =>
        $"{pilot.FirstName} {pilot.LastName}".Trim() is { Length: > 0 } fullName ? fullName : pilot.Name;
}
