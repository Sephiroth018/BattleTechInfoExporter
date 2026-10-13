using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The catalog's component definitions, which every export file's references point to, grouped by
///     <see cref="ComponentReference.Type" /> and keyed by component id: a reference's <c>type</c> names its group,
///     <c>Weapon</c> in <c>weapons</c>, <c>AmmunitionBox</c> in <c>ammunitionBoxes</c>, <c>HeatSink</c> in
///     <c>heatSinks</c>, <c>JumpJet</c> in <c>jumpJets</c> and <c>Upgrade</c> in <c>upgrades</c>. A reference whose
///     definition the game can't find has no entry; its id stands in for its name.
/// </summary>
/// <param name="Weapons">
///     The components of type <c>Weapon</c>, keyed by component id, without the game's internal melee, jump attack
///     and AI weapons, which no unit mounts.
/// </param>
/// <param name="AmmunitionBoxes">The components of type <c>AmmunitionBox</c>, keyed by component id.</param>
/// <param name="HeatSinks">The components of type <c>HeatSink</c>, keyed by component id.</param>
/// <param name="JumpJets">The components of type <c>JumpJet</c>, keyed by component id.</param>
/// <param name="Upgrades">
///     The components of type <c>Upgrade</c>, e.g. actuators, cockpit mods and gyros, keyed by component id; they
///     have only the common fields.
/// </param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ComponentDefinitions(
    IReadOnlyDictionary<string, WeaponDefinition> Weapons,
    IReadOnlyDictionary<string, AmmunitionBoxDefinition> AmmunitionBoxes,
    IReadOnlyDictionary<string, HeatSinkDefinition> HeatSinks,
    IReadOnlyDictionary<string, JumpJetDefinition> JumpJets,
    IReadOnlyDictionary<string, ComponentDefinition> Upgrades);
