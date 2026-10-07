using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds the catalog's terrain and biome definitions from the design masks (DesignMaskDef) the game has
///     loaded, which <see cref="CatalogExporter" /> loads itself. Only fields the game's combat code reads are
///     exported.
/// </summary>
internal static class TerrainReader
{
    // DesignMaskDef's default move cost, which no unit's movement reaches a single meter of.
    private const float ImpassableMoveCost = 9999.9f;

    // Painted onto cells at runtime, but never applied as a cell's terrain (MapMetaData.GetPriorityTerrainMaskFlags),
    // so their definitions' values never take effect.
    private static readonly IReadOnlyList<string> NeverAppliedMaskIds =
    [
        "DesignMaskDropshipLandingZone",
        "DesignMaskDropPodLandingZone",
        "DesignMaskDangerousLocation"
    ];

    private static readonly IReadOnlyList<Biome.BIOMESKIN> Biomes = Enum.GetValues(typeof(Biome.BIOMESKIN))
        .Cast<Biome.BIOMESKIN>()
        .Where(biome => biome != Biome.BIOMESKIN.UNDEFINED)
        .ToList();

    // Each biome's mask (MapMetaData.biomeDesignMask), which the maps name as Biome.GetDesignMaskNameFromBiomeSkin
    // does.
    private static readonly HashSet<string> BiomeMaskIds = new(
        Biomes.Select(Biome.GetDesignMaskNameFromBiomeSkin),
        StringComparer.Ordinal);

    internal static SortedDictionary<string, TerrainDefinition> ReadTerrainDefinitions(DataManager dataManager)
    {
        var terrains = new SortedDictionary<string, TerrainDefinition>(StringComparer.Ordinal);
        foreach (var mask in dataManager.DesignMaskDefs
                     .Where(mask => !BiomeMaskIds.Contains(mask.Key) && !NeverAppliedMaskIds.Contains(mask.Key)))
        {
            terrains.Add(mask.Key, ReadTerrain(mask.Value));
        }

        return terrains;
    }

    /// <summary>Keyed by the biome's id as the star systems refer to it; a biome whose mask isn't loaded has no entry.</summary>
    internal static SortedDictionary<string, BiomeDefinition> ReadBiomeDefinitions(DataManager dataManager)
    {
        var biomes = new SortedDictionary<string, BiomeDefinition>(StringComparer.Ordinal);
        foreach (var biome in Biomes)
        {
            var reference = DefinitionReferences.ReferenceTo(dataManager, biome);
            var maskId = Biome.GetDesignMaskNameFromBiomeSkin(biome);
            if (dataManager.DesignMaskDefs.TryGet(maskId, out var mask))
            {
                biomes.Add(reference.Id, ReadBiome(reference.Name, mask));
            }
            else
            {
                ModLog.Logger.LogWarning($"Left out biome {reference.Id}: its design mask {maskId} isn't loaded");
            }
        }

        return biomes;
    }

    // The move cost is PathNodeGrid.GetTerrainCost's, visibility LineOfSight.visCostOfCell's, sensors and
    // signature LineOfSight.GetAllSensorRangeMultipliers' and GetTargetSignature's, the to-hit modifiers
    // ToHit.GetTargetTerrainModifier's and GetSelfTerrainModifier's, the guard AbstractActor.HasCover's, the damage
    // taken AbstractActor.GetAdjustedDamage's and the sticky effect AbstractActor.ApplyDesignMaskStickyEffect's.
    private static TerrainDefinition ReadTerrain(DesignMaskDef mask) =>
        new(
            mask.Description.Name,
            new TerrainMoveCosts(
                new ByWeightClass<float?>(
                    MoveCost(mask.moveCostMechLight),
                    MoveCost(mask.moveCostMechMedium),
                    MoveCost(mask.moveCostMechHeavy),
                    MoveCost(mask.moveCostMechAssault)),
                new ByWeightClass<float?>(
                    MoveCost(mask.moveCostTrackedLight),
                    MoveCost(mask.moveCostTrackedMedium),
                    MoveCost(mask.moveCostTrackedHeavy),
                    MoveCost(mask.moveCostTrackedAssault)),
                new ByWeightClass<float?>(
                    MoveCost(mask.moveCostWheeledLight),
                    MoveCost(mask.moveCostWheeledMedium),
                    MoveCost(mask.moveCostWheeledHeavy),
                    MoveCost(mask.moveCostWheeledAssault))),
            mask.moveCostSprintMultiplier,
            mask.visibilityMultiplier,
            mask.visibilityHeight,
            mask.sensorRangeMultiplier,
            mask.signatureMultiplier,
            mask.targetabilityModifier,
            mask.meleeTargetabilityModifier,
            mask.toHitFromModifier,
            mask.grantsGuarded,
            mask.heatSinkMultiplier,
            mask.heatPerTurn,
            ReadDamageDealt(mask),
            new DamageMultipliers(
                mask.allDamageTakenMultiplier,
                mask.antipersonnelDamageTakenMultiplier,
                mask.energyDamageTakenMultiplier,
                mask.ballisticDamageTakenMultiplier,
                mask.missileDamageTakenMultiplier),
            mask.stickyEffect is { } stickyEffect ? EffectReader.ReadStatisticChanges([stickyEffect]) : []);

    private static float? MoveCost(float cost) => cost >= ImpassableMoveCost ? null : cost;

    // Mech.AdjustedHeatsinkCapacity, Mech.AddEnvironmentHeat and Weapon.DamagePerShotAdjusted read the biome's
    // mask; nothing else does.
    private static BiomeDefinition ReadBiome(string name, DesignMaskDef mask) =>
        new(name, mask.heatSinkMultiplier, mask.heatPerTurn, ReadDamageDealt(mask));

    // Weapon.GetMaskDamageMultiplier picks the category by the weapon category's DesignMaskString.
    private static DamageMultipliers ReadDamageDealt(DesignMaskDef mask) =>
        new(
            mask.allDamageDealtMultiplier,
            mask.antipersonnelDamageDealtMultiplier,
            mask.energyDamageDealtMultiplier,
            mask.ballisticDamageDealtMultiplier,
            mask.missileDamageDealtMultiplier);
}
