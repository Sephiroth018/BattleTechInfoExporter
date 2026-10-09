using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>The company's standing with the Mercenary Review Board, shown as its own bar in the UI.</summary>
/// <param name="Value">The rating value, which <see cref="MercenaryReviewBoardLevel.StartsAt" /> compares with.</param>
/// <param name="Level">The rating level, from 0, as in <see cref="MercenaryReviewBoardLevel.Level" />.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MercenaryReviewBoard(int Value, int Level);
