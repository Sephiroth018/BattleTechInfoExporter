using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <param name="MoraleChange">Applied once when the level is confirmed at a report.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SpendingOption(EconomyScale Level, int ExpectedExpenses, int MoraleChange);
