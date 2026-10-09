using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Level">The spending level confirmed at the last report, applying until the next one.</param>
/// <param name="Options">Every spending level, with what the next report deducts at it.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Spending(EconomyScale Level, IReadOnlyList<SpendingOption> Options);
