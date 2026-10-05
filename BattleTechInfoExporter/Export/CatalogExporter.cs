using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>The single entry point the triggers of the catalog export call.</summary>
internal static class CatalogExporter
{
    private const string CatalogFileName = "catalog.json";

    internal static void Export(SimGameState simGame, ExportTrigger trigger) =>
        CampaignExport.Run(
            simGame,
            trigger,
            () => ExportFileWriter.Write(CatalogFileName, CatalogReader.Read(simGame, trigger)));
}
