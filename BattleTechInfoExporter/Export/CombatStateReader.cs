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
        return new CombatState(
            ContractReader.ReadMissionContract(simGame, contract),
            CombatMapReader.ReadMapId(contract),
            turnDirector.CurrentRound,
            HudInitiative.FromGamePhase(turnDirector.CurrentPhase),
            combat.LocalPlayerTeam.Morale,
            units,
            ObjectiveReader.ReadObjectives(combat, units),
            ObjectiveReader.ReadZones(combat),
            BuildingReader.ReadDamagedBuildings(combat));
    }
}
