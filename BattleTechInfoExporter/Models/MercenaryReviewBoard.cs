using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The company's standing with the Mercenary Review Board, shown as its own bar in the UI.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MercenaryReviewBoard(int Value, int Level);
