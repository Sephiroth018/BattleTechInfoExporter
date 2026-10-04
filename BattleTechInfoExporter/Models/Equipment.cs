using BattleTech;
using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>A component mounted on a mech; its definition is in <see cref="GameState.ComponentDefinitions" />.</summary>
/// <param name="IsFixed">Part of the chassis; it can't be removed.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record Equipment(ComponentReference Component, ComponentDamageLevel DamageLevel, bool IsFixed);
