using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the rules file's abilities, with the statistics each one changes, from their definitions.</summary>
internal static class AbilityReader
{
    internal static SkillLevelAbility Read(SimGameState simGame, AbilityDef ability) =>
        new(
            DefinitionReferences.ReferenceTo(ability.Description),
            ability.IsPrimaryAbility,
            GameText.ToPlainText(ability.Description.Details),
            ability.ActivationTime,
            ability.Targeting == AbilityDef.TargetingType.NotSet ? null : ability.Targeting,
            ability.ActivationCooldown > 0 ? ability.ActivationCooldown : null,
            ability.NumberOfUses > 0 ? ability.NumberOfUses : null,
            EffectReader.ReadStatisticChanges(EffectsOf(simGame, ability)));

    // Coolant Vent has no effects of its own: Mech applies CoolantVentEffect when it vents, and AbstractActor
    // CoolantVentCooldownEffect when the venting ends.
    private static IEnumerable<EffectData> EffectsOf(SimGameState simGame, AbilityDef ability)
    {
        if (ability.Targeting != AbilityDef.TargetingType.ConfirmCoolantVent)
        {
            return ability.EffectData;
        }

        var resolution = simGame.CombatConstants.ResolutionConstants;
        return ability.EffectData.Append(resolution.CoolantVentEffect).Append(resolution.CoolantVentCooldownEffect);
    }
}
