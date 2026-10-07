using System;
using System.IO;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;
using Newtonsoft.Json;

namespace BattleTechInfoExporter.Export;

/// <summary>The single entry point the triggers of the catalog export call.</summary>
internal static class CatalogExporter
{
    private const string CatalogFileName = "catalog.json";

    /// <summary>
    ///     Rebuilds the catalog only when it's missing or stale: written by another mod version or from other
    ///     sources (<see cref="CatalogSourceFingerprint" />). Deleting the file forces a rebuild. The rebuild loads
    ///     every vehicle, turret and design mask, which the career doesn't (SimGameState.RequestDataManagerResources;
    ///     only a combat map loads its own masks), and writes the catalog once they are loaded: on a later frame
    ///     the first time, right away once cached.
    /// </summary>
    internal static void Export(SimGameState simGame, ExportTrigger trigger) =>
        CampaignExport.Run(
            simGame,
            trigger,
            () =>
            {
                var sourceFingerprint = CatalogSourceFingerprint.Compute(simGame.DataManager);
                if (IsCurrent(sourceFingerprint))
                {
                    ModLog.Logger.Log($"Left {CatalogFileName} unchanged ({trigger}): same mod version and sources");
                    return;
                }

                // Filtered by DLC ownership like the career's own loads; the default load weight loads no prefabs.
                // The completion may run on a later frame, outside this export's exception handling.
                var loadRequest = simGame.DataManager.CreateLoadRequest(request =>
                    CampaignExport.Run(simGame, trigger, () => Write(simGame, trigger, sourceFingerprint, request)));
                foreach (var resourceType in CatalogSourceFingerprint.ResourceTypesTheCareerDoesNotLoad)
                {
                    loadRequest.AddAllOfTypeBlindLoadRequest(resourceType, true);
                }

                loadRequest.ProcessRequests();
            });

    private static void Write(
        SimGameState simGame,
        ExportTrigger trigger,
        string sourceFingerprint,
        LoadRequest loadRequest)
    {
        foreach (var entry in loadRequest.FailedRequests)
        {
            ModLog.Logger.LogWarning($"Failed to load {entry.Type} {entry.Id} for the catalog");
        }

        ExportFileWriter.Write(CatalogFileName, CatalogReader.Read(simGame, trigger, sourceFingerprint));
    }

    // An unreadable or malformed file counts as stale; the rebuild replaces it.
    private static bool IsCurrent(string sourceFingerprint)
    {
        try
        {
            var header = ExportFileWriter.ReadHeader(
                CatalogFileName,
                nameof(Catalog.ModVersion),
                nameof(Catalog.SourceFingerprint));
            return header is not null
                   && header[nameof(Catalog.ModVersion)] == ModAssembly.Version
                   && header[nameof(Catalog.SourceFingerprint)] == sourceFingerprint;
        }
        catch (Exception exception) when (exception is JsonException or IOException)
        {
            ModLog.Logger.LogWarning($"Rebuilding the unreadable {CatalogFileName}: {exception.Message}");
            return false;
        }
    }
}
