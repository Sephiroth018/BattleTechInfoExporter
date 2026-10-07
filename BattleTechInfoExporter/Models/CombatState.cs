using System.Collections.Generic;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     The root of <c>combat-state.json</c>: the battle as the player's HUD shows it, written when every phase
///     begins and after every unit's activation; the file exists only while a battle is running.
/// </summary>
/// <param name="Contract">The contract being fought, as in <see cref="MissionOutcome.Contract" />.</param>
/// <param name="MapId">The map, keyed as in the catalog's <see cref="Catalog.MapDefinitions" />.</param>
/// <param name="Round">The combat round, counted from 1.</param>
/// <param name="Phase">
///     The current phase on the game's initiative scale, as <see cref="CombatUnitState.Initiative" />: units act
///     from 1 up, which the HUD shows counting down from 5.
/// </param>
/// <param name="Resolve">The lance's resolve now; its limits are in <c>rules.json</c>'s <see cref="ResolveRules" />.</param>
/// <param name="Units">
///     Every unit the player knows of, keyed by the game's unit id: the player's and allied units, and the enemies
///     and neutral units the HUD shows or last showed.
/// </param>
/// <param name="Objectives">The objectives the HUD lists, finished ones included, in the HUD's order.</param>
/// <param name="Zones">The zones drawn on the map.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CombatState(
    string ModVersion,
    ExportTrigger Trigger,
    MissionContract Contract,
    string? MapId,
    int Round,
    int Phase,
    int Resolve,
    IReadOnlyDictionary<string, CombatUnit> Units,
    IReadOnlyList<CombatObjective> Objectives,
    IReadOnlyList<ObjectiveZone> Zones) : ExportFile(ModVersion, null, Trigger);

/// <summary>A point on the map in meters: <see cref="Y" /> is the elevation.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record MapPosition(float X, float Y, float Z);
