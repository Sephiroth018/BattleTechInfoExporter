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

    // A category without a multiplier of its own gets only the mask's multiplier for all damage.
    private const float NoCategoryMultiplier = 1;

    private static readonly IReadOnlyList<Biome.BIOMESKIN> AllBiomes =
        Enum.GetValues(typeof(Biome.BIOMESKIN)).Cast<Biome.BIOMESKIN>().ToList();

    // The game has no description for the generic biome (DataManagerExtensions.GetBaseDescriptionDef logs an error
    // looking it up) and no map uses it.
    private static readonly IReadOnlyList<Biome.BIOMESKIN> Biomes = AllBiomes
        .Where(biome => biome is not (Biome.BIOMESKIN.UNDEFINED or Biome.BIOMESKIN.generic))
        .ToList();

    // The masks that aren't terrains: each biome's mask (MapMetaData.biomeDesignMask), which the maps name as
    // Biome.GetDesignMaskNameFromBiomeSkin does, the generic biome's included, the masks painted onto cells at
    // runtime but never applied as a cell's terrain (MapMetaData.GetPriorityTerrainMaskFlags), whose values never
    // take effect, and the map boundary's, which no playable cell has (MapMetaData.IsWithinBounds).
    private static readonly HashSet<string> NonTerrainMaskIds = new(
        AllBiomes.Select(Biome.GetDesignMaskNameFromBiomeSkin)
            .Concat(
            [
                "DesignMaskDropshipLandingZone",
                "DesignMaskDropPodLandingZone",
                "DesignMaskDangerousLocation",
                "DesignMaskMapBoundary"
            ]),
        StringComparer.Ordinal);

    internal static SortedDictionary<string, TerrainDefinition> ReadTerrainDefinitions(DataManager dataManager) =>
        CatalogEntries.ById(
            dataManager.DesignMaskDefs
                .Where(mask => !NonTerrainMaskIds.Contains(mask.Key))
                .Select(mask => ReadTerrain(mask.Value)));

    /// <summary>Keyed by the biome's id as the star systems refer to it; a biome whose mask isn't loaded has no entry.</summary>
    internal static SortedDictionary<string, BiomeDefinition> ReadBiomeDefinitions(DataManager dataManager) =>
        CatalogEntries.ById(Biomes.Select(biome => ReadBiome(dataManager, biome)).OfType<BiomeDefinition>());

    private static BiomeDefinition? ReadBiome(DataManager dataManager, Biome.BIOMESKIN biome)
    {
        var reference = DefinitionReferences.ReferenceTo(dataManager, biome);
        var maskId = Biome.GetDesignMaskNameFromBiomeSkin(biome);
        if (dataManager.DesignMaskDefs.TryGet(maskId, out var mask))
        {
            return ReadBiome(reference, mask);
        }

        ModLog.Logger.LogWarning($"Left out biome {reference.Id}: its design mask {maskId} isn't loaded");
        return null;
    }

    // The move cost is PathNodeGrid.GetTerrainCost's, visibility LineOfSight.visCostOfCell's, sensors and
    // signature LineOfSight.GetAllSensorRangeMultipliers' and GetTargetSignature's, the to-hit modifiers
    // ToHit.GetTargetTerrainModifier's and GetSelfTerrainModifier's, the guard AbstractActor.HasCover's, the damage
    // taken AbstractActor.GetAdjustedDamage's and the sticky effect AbstractActor.ApplyDesignMaskStickyEffect's.
    private static TerrainDefinition ReadTerrain(DesignMaskDef mask) =>
        new(
            mask.Id,
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
            NullIfImpassable(mask.moveCostSprintMultiplier),
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
            // AbstractActor.GetAdjustedDamage.
            ReadDamageMultipliers(
                mask.allDamageTakenMultiplier,
                mask.antipersonnelDamageTakenMultiplier,
                mask.energyDamageTakenMultiplier,
                mask.ballisticDamageTakenMultiplier,
                mask.missileDamageTakenMultiplier),
            mask.stickyEffect is { } stickyEffect ? EffectReader.ReadStatisticChanges(stickyEffect) : []);

    private static ByWeightClass<float?> MoveCosts(float light, float medium, float heavy, float assault) =>
        new(NullIfImpassable(light), NullIfImpassable(medium), NullIfImpassable(heavy), NullIfImpassable(assault));

    private static float? NullIfImpassable(float cost) => cost >= ImpassableMoveCost ? null : cost;

    // Mech.AdjustedHeatsinkCapacity, Mech.AddEnvironmentHeat and Weapon.DamagePerShotAdjusted read the biome's
    // mask; nothing else does.
    private static BiomeDefinition ReadBiome(DefinitionReference biome, DesignMaskDef mask) =>
        new(biome.Id, biome.Name, mask.heatSinkMultiplier, mask.heatPerTurn, ReadDamageDealt(mask));

    // Weapon.GetMaskDamageMultiplier.
    private static DamageMultipliers ReadDamageDealt(DesignMaskDef mask) =>
        ReadDamageMultipliers(
            mask.allDamageDealtMultiplier,
            mask.antipersonnelDamageDealtMultiplier,
            mask.energyDamageDealtMultiplier,
            mask.ballisticDamageDealtMultiplier,
            mask.missileDamageDealtMultiplier);

    // The game picks a weapon category's multiplier by its DesignMaskString; Melee and AMS match none.
    private static DamageMultipliers ReadDamageMultipliers(
        float all,
        float support,
        float energy,
        float ballistic,
        float missile) =>
        new(
            all,
            WeaponCategories.ByName(category => category.DesignMaskString switch
            {
                "Support" => support,
                "Energy" => energy,
                "Ballistic" => ballistic,
                "Missile" => missile,
                _ => NoCategoryMultiplier
            }));
}
