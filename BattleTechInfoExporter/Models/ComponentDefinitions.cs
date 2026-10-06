using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The catalog's component definitions, which every export file's references point to, grouped by
///     <see cref="ComponentReference.Type" /> and keyed by component id. A reference whose definition the game can't
///     find has no entry; its id stands in for its name.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ComponentDefinitions(
    IReadOnlyDictionary<string, WeaponDefinition> Weapons,
    IReadOnlyDictionary<string, AmmunitionBoxDefinition> AmmunitionBoxes,
    IReadOnlyDictionary<string, HeatSinkDefinition> HeatSinks,
    IReadOnlyDictionary<string, ComponentDefinition> JumpJets,
    IReadOnlyDictionary<string, ComponentDefinition> Upgrades);
