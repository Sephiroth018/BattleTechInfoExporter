using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Reads where dropships and drop pods are about to land: the cells the game marks as a landing zone, which the
///     HUD's movement reticle shows as dangerous (CombatMovementReticle.IsDangerousLocation) and whose units are
///     crushed when the landing happens.
/// </summary>
internal static class LandingZoneReader
{
    // A dropship marks its footprint while it is active and has neither landed nor left
    // (DropshipGameLogic.MarkDropshipLandingZone); a lance spawner the cells around each spawn point until its
    // drop pods have landed (LanceSpawnerGameLogic.PaintDangerousLocationForDroppods). A hex is in a zone when its
    // center cell is marked, the cell the game checks when it crushes a unit standing there
    // (UnitSpawnPointGameLogic.ApplyDropPodDamageToSquashedUnits).
    internal static List<LandingZone> Read(CombatGameState combat)
    {
        var zonesByCell = new Dictionary<MapTerrainDataCell, Zone>();
        foreach (var dropship in combat.ItemRegistry
                     .GetObjectsOfType(TaggedObjectType.ObstructionGameLogic)
                     .OfType<DropshipGameLogic>())
        {
            var zone = new Zone(dropship.encounterObjectGuid, LandingZoneKind.Dropship);
            foreach (var cell in dropship.occupiedCells
                         .Select(cell => cell.relatedTerrainCell)
                         .Where(cell => SplatMapInfo.IsDropshipLandingZone(cell.terrainMask)))
            {
                zonesByCell[cell] = zone;
            }
        }

        foreach (var spawner in combat.ItemRegistry
                     .GetObjectsOfType<LanceSpawnerGameLogic>(TaggedObjectType.LanceSpawner)
                     .Where(spawner => spawner.spawnMethod == SpawnUnitMethodType.DropPod))
        {
            var zone = new Zone(spawner.encounterObjectGuid, LandingZoneKind.DropPod);
            foreach (var cell in spawner.unitSpawnPointGameLogicList
                         .SelectMany(spawnPoint => spawnPoint.DangerousLocationCellsList)
                         .Where(cell => SplatMapInfo.IsDropPodLandingZone(cell.terrainMask)))
            {
                zonesByCell[cell] = zone;
            }
        }

        if (zonesByCell.Count == 0)
        {
            return [];
        }

        var hexesByZone = new Dictionary<Zone, List<HexCoordinates>>();
        foreach (var hex in MapHexReader.ReadPlayableHexes(combat))
        {
            if (!zonesByCell.TryGetValue(MapHexReader.CenterCell(combat, hex), out var zone))
            {
                continue;
            }

            if (!hexesByZone.TryGetValue(zone, out var hexes))
            {
                hexes = [];
                hexesByZone.Add(zone, hexes);
            }

            hexes.Add(new HexCoordinates(hex.q, hex.r));
        }

        return hexesByZone
            .OrderBy(zone => zone.Key.Id, StringComparer.Ordinal)
            .Select(zone => new LandingZone(zone.Key.Id, zone.Key.Kind, zone.Value))
            .ToList();
    }

    private sealed record Zone(string Id, LandingZoneKind Kind);
}
