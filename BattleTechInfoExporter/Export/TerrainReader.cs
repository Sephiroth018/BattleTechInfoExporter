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

    private static readonly IReadOnlyList<Biome.BIOMESKIN> AllBiomes =
        Enum.GetValues(typeof(Biome.BIOMESKIN)).Cast<Biome.BIOMESKIN>().ToList();

    // The game has no description for the generic biome (DataManagerExtensions.GetBaseDescriptionDef logs an error
    // looking it up) and no map uses it.
    private static readonly IReadOnlyList<Biome.BIOMESKIN> Biomes = AllBiomes
        .Where(biome => biome is not (Biome.BIOMESKIN.UNDEFINED or Biome.BIOMESKIN.generic))
        .ToList();

    // The masks that aren't terrains: each biome's mask (MapMetaData.biomeDesignMask), which the maps name as
    // Biome.GetDesignMaskNameFromBiomeSkin does, the generic biome's included, and the masks painted onto cells at
    // runtime but never applied as a cell's terrain (MapMetaData.GetPriorityTerrainMaskFlags), whose values never
    // take effect.
    private static readonly HashSet<string> NonTerrainMaskIds = new(
        AllBiomes.Select(Biome.GetDesignMaskNameFromBiomeSkin)
            .Concat(["DesignMaskDropshipLandingZone", "DesignMaskDropPodLandingZone", "DesignMaskDangerousLocation"]),
        StringComparer.Ordinal);

    internal static SortedDictionary<string, TerrainDefinition> ReadTerrainDefinitions(DataManager dataManager)
    {
        var terrains = new SortedDictionary<string, TerrainDefinition>(StringComparer.Ordinal);
        foreach (var mask in dataManager.DesignMaskDefs.Where(mask => !NonTerrainMaskIds.Contains(mask.Key)))
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
                MoveCosts(mask.moveCostMechLight, mask.moveCostMechMedium, mask.moveCostMechHeavy,
                    mask.moveCostMechAssault),
                MoveCosts(
                    mask.moveCostTrackedLight,
                    mask.moveCostTrackedMedium,
                    mask.moveCostTrackedHeavy,
                    mask.moveCostTrackedAssault),
                MoveCosts(
                    mask.moveCostWheeledLight,
                    mask.moveCostWheeledMedium,
                    mask.moveCostWheeledHeavy,
                    mask.moveCostWheeledAssault)),
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
            mask.stickyEffect is { } stickyEffect ? EffectReader.ReadStatisticChanges(stickyEffect) : []);

    private static ByWeightClass<float?> MoveCosts(float light, float medium, float heavy, float assault) =>
        new(MoveCost(light), MoveCost(medium), MoveCost(heavy), MoveCost(assault));

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
