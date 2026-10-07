using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>The single entry point the triggers of the catalog export call.</summary>
internal static class CatalogExporter
{
    private const string CatalogFileName = "catalog.json";

    /// <summary>
    ///     Loads every vehicle, turret and design mask, which the career doesn't (SimGameState.RequestDataManagerResources;
    ///     only a combat map loads its own masks), and writes the catalog once they are loaded: on a later frame
    ///     the first time, right away once cached.
    /// </summary>
    internal static void Export(SimGameState simGame, ExportTrigger trigger) =>
        CampaignExport.Run(
            simGame,
            trigger,
            () =>
            {
                // Filtered by DLC ownership like the career's own loads; the default load weight loads no prefabs.
                // The completion may run on a later frame, outside this export's exception handling.
                var loadRequest = simGame.DataManager.CreateLoadRequest(request =>
                    CampaignExport.Run(simGame, trigger, () => Write(simGame, trigger, request)));
                loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.VehicleDef, true);
                loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.TurretDef, true);
                loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.DesignMaskDef, true);
                loadRequest.ProcessRequests();
            });

    private static void Write(SimGameState simGame, ExportTrigger trigger, LoadRequest loadRequest)
    {
        foreach (var entry in loadRequest.FailedRequests)
        {
            ModLog.Logger.LogWarning($"Failed to load {entry.Type} {entry.Id} for the catalog");
        }

        ExportFileWriter.Write(CatalogFileName, CatalogReader.Read(simGame, trigger));
    }
}
