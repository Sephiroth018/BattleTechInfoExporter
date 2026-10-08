using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTech.Framework;
using BattleTechInfoExporter.Models;
using UnityEngine;
using Contract = BattleTechInfoExporter.Models.Contract;
using StarSystem = BattleTech.StarSystem;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds the active contract, the Command Center's contracts, the mission type table and a mission's contract
///     from the game's career state.
/// </summary>
internal static class ContractReader
{
    /// <summary>The most priority salvage picks a mission gives, hardcoded in Contract.GenerateSalvage.</summary>
    internal const int MaxPrioritySalvage = 8;

    // The slider positions of the contract negotiation in percent; their step is set in the screen's UI asset.
    private static readonly int[] NegotiationShares = [0, 25, 50, 75, 100];

    // An accepted travel contract stays active until the company proceeds with it on arrival or breaks it
    // (SimGameState.FinishCompleteBreadcrumbProcess, FailBreadcrumb).
    internal static ActiveContract? ReadActiveContract(
        SimGameState simGame,
        (StarSystem Destination, int ArrivesOnDay)? travelInProgress)
    {
        if (simGame.ActiveTravelContract is not { } contract)
        {
            return null;
        }

        var contractOverride = contract.Override;
        var (employer, target) = ReadFactions(contract);
        var starSystem = ReadStarSystem(simGame, contract);
        return new ActiveContract(
            ReadId(contractOverride),
            contractOverride.contractName,
            ReadType(simGame, contract),
            contractOverride.contractDisplayStyle,
            DefinitionReferences.ReferenceTo(employer),
            DefinitionReferences.ReferenceTo(target),
            ReadDifficulty(simGame, contractOverride),
            DefinitionReferences.ReferenceTo(starSystem.Def.Description),
            GameText.ToPlainText(contract.ShortDescription),
            ReadLanceLimits(contractOverride),
            ReadBiome(simGame, contract.ContractBiome),
            ReadTerms(simGame, contract, employer, target),
            // The arrival is the trip's as the position has it, also on the last leg from the jump point.
            travelInProgress is ({ } destination, var arrivesOnDay) && destination.ID == starSystem.ID
                ? arrivesOnDay
                : null);
    }

    // The game generates a system's contracts when the contract screen first opens there
    // (SGRoomController_CmdCenter.StartContractScreen). The mod never generates them itself: that would change the
    // career. The active contract is left out: an accepted global contract stays in SimGameState.GlobalContracts
    // until arrival, and once arrived, GetAllCurrentlySelectableContracts adds it.
    internal static List<Contract>? ReadContracts(SimGameState simGame)
    {
        if (!simGame.CurSystem.InitialContractsFetched)
        {
            return null;
        }

        return simGame.GetAllCurrentlySelectableContracts()
            .Where(contract => contract != simGame.ActiveTravelContract)
            .Select(contract => ReadContract(simGame, contract))
            .ToList();
    }

    // SimGameState.ContractTypeDescriptions has the procedural mission types; priority contracts share one entry.
    internal static List<ContractTypeDescription> ReadContractTypes(SimGameState simGame)
    {
        var contractTypes = simGame.ContractTypeDescriptions
            .Select(description => ReadContractType(description.Key, GameText.ToPlainText(description.Value.Details)))
            .ToList();
        if (simGame.PriorityMissionDescription is { } priority)
        {
            contractTypes.Add(new ContractTypeDescription(
                PriorityType(simGame),
                GameText.ToPlainText(priority.Details),
                null));
        }

        return contractTypes;
    }

    private static Contract ReadContract(SimGameState simGame, BattleTech.Contract contract)
    {
        var contractOverride = contract.Override;
        var (employer, target) = ReadFactions(contract);
        return new Contract(
            ReadId(contractOverride),
            // The contract list and details show the raw name; Contract.Name interpolates it, which the game's
            // contracts don't need.
            contractOverride.contractName,
            ReadType(simGame, contract),
            contractOverride.contractDisplayStyle,
            DefinitionReferences.ReferenceTo(employer),
            DefinitionReferences.ReferenceTo(target),
            ReadDifficulty(simGame, contractOverride),
            DefinitionReferences.ReferenceTo(ReadStarSystem(simGame, contract).Def.Description),
            // The contract details show the interpolated description, unlike the name.
            GameText.ToPlainText(contract.ShortDescription),
            ReadLanceLimits(contractOverride),
            ReadBiome(simGame, contract.ContractBiome),
            simGame.ContractUserMeetsReputation(contract),
            ReadNegotiation(simGame, contract, employer, target));
    }

    // The mission is fought in the current star system: a contract elsewhere needs travelling there first.
    internal static MissionContract ReadMissionContract(SimGameState simGame, BattleTech.Contract contract)
    {
        var contractOverride = contract.Override;
        var (employer, target) = ReadFactions(contract);
        return new MissionContract(
            ReadId(contractOverride),
            contractOverride.contractName,
            ReadType(simGame, contract),
            contractOverride.contractDisplayStyle,
            DefinitionReferences.ReferenceTo(employer),
            DefinitionReferences.ReferenceTo(target),
            ReadDifficulty(simGame, contractOverride),
            DefinitionReferences.ReferenceTo(simGame.CurSystem.Def.Description));
    }

    internal static (FactionValue Employer, FactionValue Target) ReadFactions(BattleTech.Contract contract) =>
        (contract.GetTeamFaction(contract.Override.employerTeam.teamGuid),
            contract.GetTeamFaction(contract.Override.targetTeam.teamGuid));

    // Mirrors Contract.GetContractTypeString and the type tooltip of SGContractsWidget.PopulateContract.
    private static DefinitionReference ReadType(SimGameState simGame, BattleTech.Contract contract) =>
        contract.IsPriorityContract
            ? PriorityType(simGame)
            : DefinitionReferences.ReferenceTo(contract.Override.ContractTypeValue);

    // Mirrors SimGameState.ContractUserMeetsReputation_Campaign.
    private static int ReadDifficulty(SimGameState simGame, ContractOverride contractOverride) =>
        Mathf.Min(
            contractOverride.finalDifficulty + contractOverride.difficultyUIModifier,
            MaxGlobalDifficulty(simGame));

    /// <summary>The cap on the career's global difficulty, as SimGameState.ContractUserMeetsReputation_Campaign rounds it.</summary>
    internal static int MaxGlobalDifficulty(SimGameState simGame) =>
        (int)simGame.Constants.Story.GlobalContractDifficultyMax;

    // A travel contract gets a new override without an id (SimGameState.CreateTravelContract); the original's id
    // is only kept in the success action that starts the contract on arrival.
    private static string? ReadId(ContractOverride contractOverride) =>
        !string.IsNullOrEmpty(contractOverride.ID)
            ? contractOverride.ID
            : contractOverride.OnContractSuccessResults
                .Where(result => result.Actions is not null)
                .SelectMany(result => result.Actions)
                .FirstOrDefault(action =>
                    action.Type == SimGameResultAction.ActionType.System_StartNonProceduralContract)
                ?.additionalValues[3];

    // GetContractTypeByInt returns null for an id a mod describes without enumerating it; the id stands in, with
    // no pay multiplier to read.
    private static ContractTypeDescription ReadContractType(long contractTypeId, string description)
    {
        if (ContractTypeEnumeration.GetContractTypeByInt(contractTypeId) is { } contractType)
        {
            return new ContractTypeDescription(
                DefinitionReferences.ReferenceTo(contractType),
                description,
                contractType.ContractRewardMultiplier);
        }

        ModLog.Logger.LogWarning($"Found no contract type {contractTypeId}; its id stands in");
        var id = contractTypeId.ToString(CultureInfo.InvariantCulture);
        return new ContractTypeDescription(new DefinitionReference(id, id), description, null);
    }

    private static DefinitionReference PriorityType(SimGameState simGame) =>
        new("Priority", simGame.PriorityMissionTitle);

    // Accepting a contract that can't be negotiated sets its fixed shares (SGContractsWidget.OnContractAccepted).
    private static Negotiation ReadNegotiation(
        SimGameState simGame,
        BattleTech.Contract contract,
        FactionValue employer,
        FactionValue target)
    {
        if (!contract.CanNegotiate)
        {
            return new Negotiation(
                false,
                null,
                ReadNegotiationOption(
                    simGame,
                    contract,
                    employer,
                    target,
                    null,
                    null,
                    contract.Override.negotiatedSalary,
                    contract.Override.negotiatedSalvage));
        }

        // Pay, salvage and reputation each depend only on their own share. The sliders' shares can't exceed 100
        // together; without employer reputation they are coupled and leave nothing for it
        // (SGContractsWidget.OnNegPaymentChange, ShouldAdjustReputation). Accepting sets the reputation share to the
        // rest (Contract.SetNegotiatedValues).
        return new Negotiation(
            true,
            NegotiationShares
                .Select(share =>
                {
                    var fraction = share / 100f;
                    return new ValuesAtShare(
                        share,
                        ReadPay(simGame, contract, fraction),
                        ReadSalvage(simGame, contract, fraction),
                        ReadReputation(simGame, contract, employer, target, fraction));
                })
                .ToList(),
            null);
    }

    // Accepting a contract stores the shares it was accepted with (SGContractsWidget.OnContractAccepted).
    private static NegotiationOption ReadTerms(
        SimGameState simGame,
        BattleTech.Contract contract,
        FactionValue employer,
        FactionValue target) =>
        ReadNegotiationOption(
            simGame,
            contract,
            employer,
            target,
            contract.CanNegotiate ? Mathf.RoundToInt(contract.PercentageContractValue * 100) : null,
            contract.CanNegotiate ? Mathf.RoundToInt(contract.PercentageContractSalvage * 100) : null,
            contract.PercentageContractValue,
            contract.PercentageContractSalvage);

    private static NegotiationOption ReadNegotiationOption(
        SimGameState simGame,
        BattleTech.Contract contract,
        FactionValue employer,
        FactionValue target,
        int? payPercent,
        int? salvagePercent,
        float payShare,
        float salvageShare) =>
        new(
            payPercent,
            salvagePercent,
            ReadPay(simGame, contract, payShare),
            ReadSalvage(simGame, contract, salvageShare),
            ReadReputation(simGame, contract, employer, target, 1f - (payShare + salvageShare)));

    // Mirrors SimGameState.GetScaledCBillValue, as SGContractsWidget.UpdateCurrentValues shows the pay.
    private static int ReadPay(SimGameState simGame, BattleTech.Contract contract, float share) =>
        simGame.GetScaledCBillValue(contract.InitialContractValue, share * contract.InitialContractValue);

    // Mirrors SGContractsWidget.UpdateCurrentValues. Its priority share is a hardcoded quarter; this takes the
    // constant the mission's outcome uses (Contract.FinalizeSalvage), 0.25 in the game's data.
    private static Salvage ReadSalvage(SimGameState simGame, BattleTech.Contract contract, float share)
    {
        var potential = contract.Override.salvagePotential > -1 ? contract.Override.salvagePotential
            : contract.SalvagePotential > -1 ? contract.SalvagePotential
            : simGame.Constants.Salvage.DefaultSalvagePotential;
        var total = potential > 0
            ? Mathf.FloorToInt(potential * share) + simGame.Constants.Finances.ContractFloorSalvageBonus
            : potential;
        return new Salvage(
            total,
            Mathf.Min(Mathf.FloorToInt(total * simGame.Constants.Salvage.PrioritySalvageModifier), MaxPrioritySalvage));
    }

    // Mirrors SGContractsReputationNegotiationWidget.ReputationAdjustment, which derives the target's change from
    // the employer's and shows each only for a faction that gains reputation.
    private static ReputationChange ReadReputation(
        SimGameState simGame,
        BattleTech.Contract contract,
        FactionValue employer,
        FactionValue target,
        float share)
    {
        var employerChange = contract.GetCurrentReputationValue(simGame.Constants, share);
        return new ReputationChange(
            employer.DoesGainReputation ? employerChange : null,
            target.DoesGainReputation
                ? Mathf.RoundToInt(employerChange * simGame.Constants.Story.TargetRepSuccessMod)
                : null);
    }

    // As SGContractsWidget.PopulateContract passes them to the lance tonnage icons; -1 means no limit.
    private static LanceLimits ReadLanceLimits(ContractOverride contractOverride)
    {
        var mechs = contractOverride.mechMinTonnages
            .Zip(contractOverride.mechMaxTonnages, (min, max) => new MechSlotLimits(LimitOf(min), LimitOf(max)))
            .Take(contractOverride.maxNumberOfPlayerUnits)
            .ToList();
        return new LanceLimits(
            contractOverride.maxNumberOfPlayerUnits,
            LimitOf(contractOverride.lanceMinTonnage),
            LimitOf(contractOverride.lanceMaxTonnage),
            mechs.Any(limits => limits.MinTonnage is not null || limits.MaxTonnage is not null) ? mechs : null);
    }

    private static float? LimitOf(float tonnage) => tonnage < 0 ? null : tonnage;

    // The contract screens show only biomes above generic (SGContractsWidget.PopulateContract,
    // LanceContractDetailsWidget).
    private static DefinitionReference? ReadBiome(SimGameState simGame, Biome.BIOMESKIN biome) =>
        biome <= Biome.BIOMESKIN.generic ? null : DefinitionReferences.ReferenceTo(simGame.DataManager, biome);

    // The contract list marks travel contracts by the target star system in the contract's context
    // (SGContractsListItem); without one, the contract is in the current star system.
    private static StarSystem ReadStarSystem(SimGameState simGame, BattleTech.Contract contract) =>
        contract.GameContext.GetObject(GameContextObjectTagEnum.TargetStarSystem) as StarSystem ?? simGame.CurSystem;
}
