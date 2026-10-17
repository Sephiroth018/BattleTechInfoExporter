using System;

namespace BattleTechInfoExporter.Export;

/// <summary>Rounds the game's floats for export, so accumulated float noise (34.3999977 for 34.4) doesn't show.</summary>
internal static class Rounding
{
    // A tenth is finer than any damage, height or percent the game computes with.
    internal static float ToTenth(float value) => (float)Math.Round(value, 1);
}
