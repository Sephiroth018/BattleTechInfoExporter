using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the last lance from the game's career state.</summary>
internal static class LanceReader
{
    // Mirrors SimGameState.GetLastLance, which reads the first four units and drops those where neither the mech
    // nor the pilot still exists. SaveLastLance stores only units with both, so the two lists have the same length.
    internal static List<LastLanceUnit> ReadLastLance(SimGameState simGame) =>
        simGame.LastUsedMechs is null || simGame.LastUsedPilots is null
            ? []
            : simGame.LastUsedMechs
                .Zip(
                    simGame.LastUsedPilots,
                    (mechId, pilotId) => (mech: simGame.GetMechByID(mechId), pilot: simGame.GetPilot(pilotId)))
                .Take(4)
                .Where(unit => unit.mech is not null || unit.pilot is not null)
                .Select(unit => new LastLanceUnit(
                    unit.mech is null ? null : MechReader.ReferenceToBayMech(unit.mech),
                    unit.pilot is null ? null : PilotReader.ReferenceTo(unit.pilot)))
                .ToList();
}
