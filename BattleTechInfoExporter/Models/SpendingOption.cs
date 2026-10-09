using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Level">The spending level, whose cost multiplier and morale change <see cref="SpendingLevelRules" /> has.</param>
/// <param name="ExpectedExpenses">
///     The C-Bills the next report deducts at this level, as <see cref="Models.ExpectedExpenses.Total" /> does at the
///     current one.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SpendingOption(EconomyScale Level, int ExpectedExpenses);
