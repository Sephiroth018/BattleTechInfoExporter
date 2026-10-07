using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="Level">The spending level, whose cost multiplier and morale change <see cref="SpendingLevelRules" /> has.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SpendingOption(EconomyScale Level, int ExpectedExpenses);
