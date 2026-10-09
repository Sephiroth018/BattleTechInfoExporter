using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The catalog's component definitions, which every export file's references point to, grouped by
///     <see cref="ComponentReference.Type" /> and keyed by component id. A reference whose definition the game can't
///     find has no entry; its id stands in for its name.
/// </summary>
/// <param name="Weapons">
///     The weapons, keyed by component id, without the game's internal melee, jump attack and AI weapons, which no
///     unit mounts.
/// </param>
/// <param name="AmmunitionBoxes">The ammunition boxes, keyed by component id.</param>
/// <param name="HeatSinks">The heat sinks, keyed by component id.</param>
/// <param name="JumpJets">The jump jets, keyed by component id.</param>
/// <param name="Upgrades">
///     The upgrades, e.g. actuators, cockpit mods and gyros, keyed by component id; they have only the common fields.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ComponentDefinitions(
    IReadOnlyDictionary<string, WeaponDefinition> Weapons,
    IReadOnlyDictionary<string, AmmunitionBoxDefinition> AmmunitionBoxes,
    IReadOnlyDictionary<string, HeatSinkDefinition> HeatSinks,
    IReadOnlyDictionary<string, JumpJetDefinition> JumpJets,
    IReadOnlyDictionary<string, ComponentDefinition> Upgrades);
