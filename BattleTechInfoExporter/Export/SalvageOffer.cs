using System.Collections.Generic;
using BattleTech;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     The salvage on offer as <see cref="Contract.CompleteContract" /> leaves it, taken before
///     <see cref="Contract.FinalizeSalvage" /> moves the priority and the random salvage out of the pool.
/// </summary>
/// <param name="Pool">The items to choose from, as <see cref="Contract.GetPotentialSalvage" /> groups them.</param>
/// <param name="Automatic">
///     <see cref="Contract.SalvageResults" /> before the choice: only the components recovered from the company's
///     lost mechs.
/// </param>
internal sealed record SalvageOffer(IReadOnlyList<SalvageDef> Pool, IReadOnlyList<SalvageDef> Automatic);
