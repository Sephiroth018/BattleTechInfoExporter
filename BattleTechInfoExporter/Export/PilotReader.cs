using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using Pilot = BattleTech.Pilot;

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

    // Wrapped in a Pilot as SG_HiringHall_Screen.AddPeople does, which gives the skills, abilities and health.
    internal static List<HiringHallPilot> ReadHiringHall(SimGameState simGame) =>
        simGame.CurSystem.AvailablePilots
            .Select(definition => ReadHiringHallPilot(
                simGame,
                new Pilot(definition, definition.Description.FullName(), true)))
            .ToList();

    internal static PilotReference ReferenceTo(Pilot pilot)
    {
        var description = pilot.pilotDef.Description;
        return new PilotReference(description.Id, FullName(description), description.Callsign);
    }

    private static HiringHallPilot ReadHiringHallPilot(SimGameState simGame, Pilot pilot)
    {
        var definition = pilot.pilotDef;
        return new HiringHallPilot(
            ReadPilotCommon(simGame, pilot),
            // What SG_HiringHall_Screen shows and SimGameState.HirePilot charges.
            simGame.CurSystem.GetPurchaseCostAfterReputationModifier(simGame.GetMechWarriorHiringCost(definition)),
            FinancesReader.ReadSalary(simGame, definition),
            simGame.CanMechWarriorBeHiredAccordingToMRBRating(pilot)
            && simGame.CanMechWarriorBeHiredAccordingToMorale(pilot));
    }

    private static BarracksPilot ReadPilot(SimGameState simGame, Pilot pilot)
    {
        var definition = pilot.pilotDef;
        return new BarracksPilot(
            ReadPilotCommon(simGame, pilot),
            new Experience(pilot.UnspentXP, pilot.SpentXP),
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

    private static Models.Pilot ReadPilotCommon(SimGameState simGame, Pilot pilot)
    {
        var description = pilot.pilotDef.Description;
        return new Models.Pilot(
            description.Id,
            FullName(description),
            description.Callsign,
            ReadPilotType(simGame, pilot),
            description.Age,
            description.Gender,
            simGame.GetPilotFullExpertise(pilot),
            ReadSkills(pilot),
            ReadAbilities(pilot.pilotDef),
            pilot.Health);
    }

    private static Skills ReadSkills(Pilot pilot) => new(pilot.Gunnery, pilot.Piloting, pilot.Guts, pilot.Tactics);

    private static List<DefinitionReference> ReadAbilities(PilotDef pilot) =>
        SimGameState.GetPrimaryPilotAbilities(pilot)
            .Select(ability => DefinitionReferences.ReferenceTo(ability.Description))
            .ToList();

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
