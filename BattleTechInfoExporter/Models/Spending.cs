using System.Collections.Generic;
using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The company's spending level, which scales the expenses of each financial report and changes morale.
/// </summary>
/// <param name="Level">The spending level confirmed at the last report, applying until the next one.</param>
/// <param name="Options">Every spending level, with what the next report deducts at it.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Spending(EconomyScale Level, IReadOnlyList<SpendingOption> Options);
