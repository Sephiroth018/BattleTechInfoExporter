using System;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The properties every export file starts with.</summary>
/// <param name="ExportedAt">
///     When the file was last changed; <c>null</c> until <see cref="Export.ExportFileWriter" /> writes it.
/// </param>
/// <param name="Trigger">What caused the export that last changed the file.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal abstract record ExportFile(string ModVersion, DateTimeOffset? ExportedAt, ExportTrigger Trigger);
