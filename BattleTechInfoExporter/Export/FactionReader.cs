using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTechInfoExporter.Models;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Reads the factions' relations for the catalog, and which factions hire the company against whom in a star
///     system for the star systems file.
/// </summary>
internal static class FactionReader
{
    internal static SortedDictionary<string, FactionDefinition> ReadFactionDefinitions(DataManager dataManager) =>
        CatalogEntries.ById(
            dataManager.Factions
                .Select(faction => ReadFactionDefinition(faction.Value))
                .OfType<FactionDefinition>());

    // A definition whose faction isn't in the enumeration is never used by the game (SimGameState.GetFactionDef finds
    // factions by their enumeration name). One whose faction names another definition (FactionValue.FactionDefID),
    // like faction_AuriganBetrayers, whose faction names the Directorate's, is one no reference leads to.
    private static FactionDefinition? ReadFactionDefinition(FactionDef faction)
    {
        var value = faction.FactionValue;
        if (value.IsInvalidUnset)
        {
            ModLog.Logger.LogWarning($"Left out faction definition {faction.ID}: its faction isn't in the enumeration");
            return null;
        }

        if (value.FactionDefID != faction.ID)
        {
            return null;
        }

        var reference = DefinitionReferences.ReferenceTo(value);
        return new FactionDefinition(
            reference.Id,
            reference.Name,
            ReferencesTo(faction.Allies),
            ReferencesTo(faction.Enemies));
    }

    // A modded definition may leave its relations out, which the game reads as none where it checks
    // (SimGameState.GetAllEnemies).
    private static List<DefinitionReference> ReferencesTo(string[]? factionNames) =>
        (factionNames ?? [])
        .Distinct()
        .Select(FactionEnumeration.GetFactionByName)
        .Where(faction => !faction.IsInvalidUnset)
        .Select(DefinitionReferences.ReferenceTo)
        .OrderBy(faction => faction)
        .ToList();

    /// <summary>
    ///     The factions that can hire the company in the star system, each with the factions it can hire it against
    ///     there, as <see cref="SimGameState.GenerateContractParticipants" /> pairs them.
    /// </summary>
    // That method can't be called instead: it also draws each pair's other factions from the game's NetworkRandom,
    // which would change the game's draws.
    internal static IReadOnlyList<ContractEmployer> ReadContractEmployers(
        SimGameState simGame,
        StarSystemDef starSystem) =>
        starSystem.ContractEmployerIDList
            .Distinct()
            .Where(employer => !simGame.ignoredContractEmployers.Contains(employer))
            .Select(employer => ReadContractEmployer(simGame, starSystem, employer))
            .OfType<ContractEmployer>()
            .OrderBy(employer => employer.Employer)
            .ToList();

    // Without a target the employer gives no contracts (SimGameState.GetValidParticipants).
    private static ContractEmployer? ReadContractEmployer(
        SimGameState simGame,
        StarSystemDef starSystem,
        string employerName)
    {
        if (FindFactionDefinition(simGame, starSystem, employerName) is not { } employer)
        {
            return null;
        }

        var targets = (employer.Enemies ?? [])
            .Distinct()
            .Where(target => starSystem.ContractTargetIDList.Contains(target)
                             && !simGame.IgnoredContractTargets.Contains(target))
            .Select(target => FindFactionDefinition(simGame, starSystem, target))
            .OfType<FactionDef>()
            .Select(target => target.FactionValue)
            .Where(target => !simGame.IsFactionAlly(target))
            .Select(DefinitionReferences.ReferenceTo)
            .OrderBy(target => target)
            .ToList();
        return targets.Count == 0
            ? null
            : new ContractEmployer(DefinitionReferences.ReferenceTo(employer.FactionValue), targets);
    }

    // The game looks the employers and targets up without a check and fails on a missing one; the export leaves it out
    // instead, so one modded star system doesn't fail the whole export.
    private static FactionDef? FindFactionDefinition(SimGameState simGame, StarSystemDef starSystem, string faction)
    {
        var definition = simGame.GetFactionDef(faction);
        if (definition is null)
        {
            ModLog.Logger.LogWarning(
                $"Left out faction {faction} of {starSystem.Description.Id}'s contracts: it has no definition");
        }

        return definition;
    }
}
