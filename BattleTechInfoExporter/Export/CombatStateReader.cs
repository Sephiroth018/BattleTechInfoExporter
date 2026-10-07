using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the combat state file's model from the running battle.</summary>
internal static class CombatStateReader
{
    internal static CombatState Read(SimGameState simGame, CombatGameState combat, ExportTrigger trigger)
    {
        var contract = combat.ActiveContract;
        var turnDirector = combat.TurnDirector;
        var units = CombatUnitReader.ReadUnits(combat);
        return new CombatState(
            ModAssembly.Version,
            trigger,
            ContractReader.ReadMissionContract(simGame, contract),
            // The catalog keys maps by their MapID, which the contract knows only by its path.
            MetadataDatabase.Instance.GetMapByPath(contract.mapPath)?.MapID,
            turnDirector.CurrentRound,
            HudInitiative.FromGamePhase(turnDirector.CurrentPhase),
            combat.LocalPlayerTeam.Morale,
            units,
            ObjectiveReader.ReadObjectives(combat, units),
            ObjectiveReader.ReadZones(combat));
    }
}
