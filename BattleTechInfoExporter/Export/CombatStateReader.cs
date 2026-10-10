using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the combat state file's model from the running battle.</summary>
internal static class CombatStateReader
{
    internal static CombatState Read(SimGameState simGame, CombatGameState combat)
    {
        var contract = combat.ActiveContract;
        var turnDirector = combat.TurnDirector;
        var units = CombatUnitReader.ReadUnits(combat);
        var objectives = ObjectiveReader.ReadObjectives(combat, units);
        return new CombatState(
            ContractReader.ReadMissionContract(simGame, contract),
            MapReader.ReadMapId(contract),
            turnDirector.CurrentRound,
            HudInitiative.FromGamePhase(turnDirector.CurrentPhase),
            combat.LocalPlayerTeam.Morale,
            MovementReader.Legend,
            units,
            objectives,
            ObjectiveReader.ReadZones(combat, objectives),
            BuildingReader.ReadSides(combat),
            BuildingReader.ReadDamagedBuildings(combat));
    }
}
