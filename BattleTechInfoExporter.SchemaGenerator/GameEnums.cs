using System;
using System.Collections.Generic;

namespace BattleTechInfoExporter.SchemaGenerator;

/// <summary>
///     What the schemas say of the game's enums the export writes, which have no doc comments of the mod's: a
///     description, and the values the mod's readers never write.
/// </summary>
internal static class GameEnums
{
    // Keyed by full name, since the generator doesn't compile against the game's assemblies.
    private static readonly Dictionary<string, GameEnum> ByFullName = new()
    {
        ["BattleTech.AbilityDef+ActivationTiming"] =
            new GameEnum("When an ability takes effect, e.g. always or on command."),
        // AbilityReader writes no targeting for NotSet.
        ["BattleTech.AbilityDef+TargetingType"] = new GameEnum("How an ability is aimed.", ["NotSet"]),
        ["BattleTech.ArmorLocation"] = new GameEnum("A mech's armor location, the torso's front and rear apart."),
        ["BattleTech.ChassisLocations"] = new GameEnum("A mech's body location."),
        ["BattleTech.ComponentDamageLevel"] = new GameEnum("How damaged a component is."),
        ["BattleTech.ComponentType"] =
            new GameEnum(
                "The game's kind of component, which names the component's group in catalog.json "
                + "componentDefinitions."),
        ["BattleTech.Contract+ContractState"] = new GameEnum("Where a contract stands, or how its mission ended."),
        ["BattleTech.DeathMethod"] = new GameEnum("How a unit was destroyed, as the game records it."),
        ["BattleTech.DropshipLocation"] = new GameEnum("A room of the company's ship."),
        ["BattleTech.DropshipType"] = new GameEnum("The company's ship: the Leopard, later the Argo."),
        ["BattleTech.EconomyScale"] = new GameEnum("A spending level, from the cheapest to the most generous."),
        // EffectReader replaces NotSet with the target the game acts on.
        ["BattleTech.EffectTargetType"] =
            new GameEnum("Whose statistic a change applies to, relative to the unit with the ability or component.",
                ["NotSet"]),
        ["BattleTech.EffectTriggerType"] = new GameEnum("When the game makes a statistic change."),
        ["BattleTech.EventScope"] =
            new GameEnum(
                "Who or what an event, a requirement or a result is about: the company, a pilot, a mech or the star "
                + "system."),
        ["BattleTech.Framework.ContractDisplayStyle"] =
            new GameEnum("Whether a contract is a regular, story, restoration or flashpoint contract."),
        ["BattleTech.Framework.ObjectiveStatus"] = new GameEnum("Where an objective stands."),
        ["BattleTech.Gender"] = new GameEnum("A pilot's gender, which picks the wording of the texts about them."),
        ["BattleTech.Operator"] = new GameEnum("How a statistic's value compares to the required value."),
        ["BattleTech.SimGameReputation"] =
            new GameEnum("A reputation level, described in rules.json reputationLevels."),
        ["BattleTech.SimGameResultAction+ActionType"] = new GameEnum("What an event result's action does."),
        ["BattleTech.SimGameTravelStatus"] = new GameEnum("Where the ship is on a trip, or in a star system."),
        ["BattleTech.StatCollection+StatOperation"] =
            new GameEnum("How a statistic change applies its value to the statistic."),
        // EffectReader writes no target collection for NotSet.
        ["BattleTech.StatisticEffectData+TargetCollection"] =
            new GameEnum(
                "Which of a unit's parts a statistic change applies to: its pilot or a group of its components.",
                ["NotSet"]),
        ["BattleTech.StrayShotValidTargets"] = new GameEnum("Which units a missed shot can hit instead."),
        ["BattleTech.VehicleChassisLocations"] = new GameEnum("A vehicle's location."),
        ["BattleTech.VehicleMovementType"] =
            new GameEnum("How a vehicle moves, which picks its row of a terrain's move costs."),
        // EffectReader filters by weapon sub type or type only where it is set.
        ["BattleTech.WeaponSubType"] =
            new GameEnum("A weapon's sub type, e.g. a weapon model or a melee attack.", ["NotSet"]),
        ["BattleTech.WeaponType"] = new GameEnum("A weapon's type, e.g. a weapon family or melee.", ["NotSet"]),
        ["BattleTech.WeightClass"] = new GameEnum("A unit's weight class."),
        ["BattleTech.WorkOrderType"] = new GameEnum("The game's kind of work order on the timeline.")
    };

    /// <summary>What the schemas say of the game's enum; <c>null</c> where the table has nothing on it.</summary>
    internal static GameEnum? Of(Type gameEnum) => ByFullName.GetValueOrDefault(gameEnum.FullName!);
}

/// <param name="Description">The enum's description.</param>
/// <param name="NeverWritten">The values the mod's readers never write, by name.</param>
internal sealed record GameEnum(string Description, IReadOnlyCollection<string> NeverWritten)
{
    internal GameEnum(string description) : this(description, [])
    {
    }
}
