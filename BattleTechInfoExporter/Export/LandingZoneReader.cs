using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;
using HBS.Math;

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
    // drop pods have landed (LanceSpawnerGameLogic.PaintDangerousLocationForDroppods).
    internal static List<LandingZone> Read(CombatGameState combat)
    {
        var dropshipZones = combat.ItemRegistry
            .GetObjectsOfType(TaggedObjectType.ObstructionGameLogic)
            .OfType<DropshipGameLogic>()
            .Select(dropship => ReadZone(
                combat,
                dropship.GUID,
                LandingZoneKind.Dropship,
                dropship.occupiedCells
                    .Select(cell => cell.relatedTerrainCell)
                    .Where(cell => SplatMapInfo.IsDropshipLandingZone(cell.terrainMask)),
                ReadFootprintHexes(combat, dropship)));
        var dropPodZones = combat.ItemRegistry
            .GetObjectsOfType<LanceSpawnerGameLogic>(TaggedObjectType.LanceSpawner)
            .Where(spawner => spawner.spawnMethod == SpawnUnitMethodType.DropPod)
            .Select(spawner => ReadZone(
                combat,
                spawner.GUID,
                LandingZoneKind.DropPod,
                spawner.unitSpawnPointGameLogicList
                    .SelectMany(spawnPoint => spawnPoint.DangerousLocationCellsList)
                    .Where(cell => SplatMapInfo.IsDropPodLandingZone(cell.terrainMask)),
                spawner.unitSpawnPointGameLogicList.SelectMany(spawnPoint => ReadPodHexes(combat, spawnPoint))));
        return dropshipZones
            .Concat(dropPodZones)
            .OfType<LandingZone>()
            .OrderBy(zone => zone.Id, StringComparer.Ordinal)
            .ToList();
    }

    // A hex is in a zone when its center cell is marked, the cell the game checks when it crushes a unit standing
    // there (UnitSpawnPointGameLogic.ApplyDropPodDamageToSquashedUnits); a zone without such a hex is left out.
    private static LandingZone? ReadZone(
        CombatGameState combat,
        string id,
        LandingZoneKind kind,
        IEnumerable<MapTerrainDataCell> markedCells,
        IEnumerable<HexPoint3> candidateHexes)
    {
        var markedCellSet = new HashSet<MapTerrainDataCell>(markedCells);
        if (markedCellSet.Count == 0)
        {
            return null;
        }

        var hexes = candidateHexes
            .Distinct()
            .Where(hex => markedCellSet.Contains(MapHexReader.CenterCell(combat, hex)))
            .OrderBy(hex => hex.r)
            .ThenBy(hex => hex.q)
            .Select(hex => new HexCoordinates(hex.q, hex.r))
            .ToList();
        return hexes.Count == 0 ? null : new LandingZone(id, kind, hexes);
    }

    // The footprint's cells span the bounds the game keeps for its line of sight targets, measured at the cells'
    // low corners (ObstructionGameLogic.AddMapEncounterLayerDataCell), so a cell's width is added at the far ends.
    private static IEnumerable<HexPoint3> ReadFootprintHexes(CombatGameState combat, DropshipGameLogic dropship)
    {
        var bounds = dropship.losTargetCalcs;
        return bounds.minX > bounds.maxX
            ? []
            : MapHexReader.ReadPlayableHexesWithin(
                combat,
                bounds.minX,
                bounds.maxX + MapMetaDataExporter.cellSize,
                bounds.minZ,
                bounds.maxZ + MapMetaDataExporter.cellSize);
    }

    // A pod marks the cell at its spawn point, which is snapped to the hex grid (WorldPointGameLogic), and the
    // ring of cells around it: the hex it lands on.
    private static IEnumerable<HexPoint3> ReadPodHexes(CombatGameState combat, UnitSpawnPointGameLogic spawnPoint)
    {
        var reach = 2f * MapMetaDataExporter.cellSize;
        var center = spawnPoint.hexPosition;
        return MapHexReader.ReadPlayableHexesWithin(
            combat,
            center.x - reach,
            center.x + reach,
            center.z - reach,
            center.z + reach);
    }
}
