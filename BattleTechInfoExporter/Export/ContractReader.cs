using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using BattleTech.Framework;
using BattleTechInfoExporter.Models;
using HBS.Nav;
using UnityEngine;
using Contract = BattleTechInfoExporter.Models.Contract;

namespace BattleTechInfoExporter.Export;

/// <summary>
///     Builds the active contract, the Command Center's contracts and the mission type table from the game's career
///     state.
/// </summary>
internal static class ContractReader
{
    // The slider positions of the contract negotiation in percent; their step is set in the screen's UI asset.
    private static readonly int[] NegotiationShares = [0, 25, 50, 75, 100];

    // An accepted travel contract stays active until the company proceeds with it on arrival or breaks it
    // (SimGameState.FinishCompleteBreadcrumbProcess, FailBreadcrumb).
    internal static ActiveContract? ReadActiveContract(
        SimGameState simGame,
        (StarSystem Destination, int DaysLeft)? travelInProgress)
    {
        if (simGame.ActiveTravelContract is not { } contract)
        {
            return null;
        }

        var contractOverride = contract.Override;
        var employer = contract.GetTeamFaction(contractOverride.employerTeam.teamGuid);
        var target = contract.GetTeamFaction(contractOverride.targetTeam.teamGuid);
        return new ActiveContract(
            ReadId(contractOverride),
            contractOverride.contractName,
            contract.ShortDescription,
            ReadType(simGame, contract),
            contractOverride.contractDisplayStyle,
            DefinitionReferences.ReferenceTo(employer),
            DefinitionReferences.ReferenceTo(target),
            ReadDifficulty(simGame, contractOverride),
            ReadTerms(simGame, contract, employer, target),
            ReadLanceLimits(contractOverride),
            ReadBiome(simGame, contract.ContractBiome),
            ReadActiveContractTravel(simGame, contract, travelInProgress));
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

        // Contracts often share a target system, whose tags and route are costly to read; each is read once.
        var travelsBySystemId = new Dictionary<string, ContractTravel?>();
        return simGame.GetAllCurrentlySelectableContracts()
            .Where(contract => contract != simGame.ActiveTravelContract)
            .Select(contract => ReadContract(simGame, contract, ReadTravelOnce(contract)))
            .ToList();

        ContractTravel? ReadTravelOnce(BattleTech.Contract contract)
        {
            if (ReadTargetSystem(contract) is not { } system)
            {
                return null;
            }

            if (!travelsBySystemId.TryGetValue(system.ID, out var travel))
            {
                travel = ReadTravel(simGame, contract);
                travelsBySystemId.Add(system.ID, travel);
            }

            return travel;
        }
    }

    // SimGameState.ContractTypeDescriptions has the procedural mission types; priority contracts share one entry.
    internal static List<ContractTypeDescription> ReadContractTypes(SimGameState simGame)
    {
        var contractTypes = simGame.ContractTypeDescriptions
            .Select(description => new ContractTypeDescription(
                ReadContractType(description.Key),
                description.Value.Details))
            .ToList();
        if (simGame.PriorityMissionDescription is { } priority)
        {
            contractTypes.Add(new ContractTypeDescription(PriorityType(simGame), priority.Details));
        }

        return contractTypes;
    }

    private static Contract ReadContract(SimGameState simGame, BattleTech.Contract contract, ContractTravel? travel)
    {
        var contractOverride = contract.Override;
        var employer = contract.GetTeamFaction(contractOverride.employerTeam.teamGuid);
        var target = contract.GetTeamFaction(contractOverride.targetTeam.teamGuid);
        return new Contract(
            ReadId(contractOverride),
            // The contract list and details show the raw name; Contract.Name interpolates it, which the game's
            // contracts don't need.
            contractOverride.contractName,
            // The contract details show the interpolated description, unlike the name.
            contract.ShortDescription,
            ReadType(simGame, contract),
            contractOverride.contractDisplayStyle,
            DefinitionReferences.ReferenceTo(employer),
            DefinitionReferences.ReferenceTo(target),
            ReadDifficulty(simGame, contractOverride),
            simGame.ContractUserMeetsReputation(contract),
            ReadNegotiation(simGame, contract, employer, target),
            ReadLanceLimits(contractOverride),
            ReadBiome(simGame, contract.ContractBiome),
            travel);
    }

    // Mirrors Contract.GetContractTypeString and the type tooltip of SGContractsWidget.PopulateContract.
    private static DefinitionReference ReadType(SimGameState simGame, BattleTech.Contract contract) =>
        contract.IsPriorityContract ? PriorityType(simGame) : ReferenceTo(contract.Override.ContractTypeValue);

    // Mirrors SimGameState.ContractUserMeetsReputation_Campaign.
    private static int ReadDifficulty(SimGameState simGame, ContractOverride contractOverride) =>
        Mathf.Min(
            contractOverride.finalDifficulty + contractOverride.difficultyUIModifier,
            (int)simGame.Constants.Story.GlobalContractDifficultyMax);

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

    // GetContractTypeByInt returns null for an id a mod describes without enumerating it; the id stands in.
    private static DefinitionReference ReadContractType(long contractTypeId)
    {
        if (ContractTypeEnumeration.GetContractTypeByInt(contractTypeId) is { } contractType)
        {
            return ReferenceTo(contractType);
        }

        ModLog.Logger.LogWarning($"Found no contract type {contractTypeId}; its id stands in");
        var id = contractTypeId.ToString(CultureInfo.InvariantCulture);
        return new DefinitionReference(id, id);
    }

    private static DefinitionReference ReferenceTo(ContractTypeValue contractType) =>
        new(contractType.Name, contractType.FriendlyName);

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
            Mathf.Min(Mathf.FloorToInt(total * simGame.Constants.Salvage.PrioritySalvageModifier), 8));
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

    private static ContractTravel? ReadTravel(SimGameState simGame, BattleTech.Contract contract) =>
        ReadTargetSystem(contract) is { } system && system != simGame.CurSystem
            ? CreateTravel(system, ReadTravelDays(simGame, system))
            : null;

    // While travelling to the contract's system, the days are the trip's as the position has them, also on the last
    // leg from the jump point to the planet.
    private static ContractTravel? ReadActiveContractTravel(
        SimGameState simGame,
        BattleTech.Contract contract,
        (StarSystem Destination, int DaysLeft)? travelInProgress) =>
        ReadTargetSystem(contract) is { } system && travelInProgress?.Destination.ID == system.ID
            ? CreateTravel(system, travelInProgress.Value.DaysLeft)
            : ReadTravel(simGame, contract);

    // The contract list marks travel contracts by the target system in the contract's context (SGContractsListItem).
    private static StarSystem? ReadTargetSystem(BattleTech.Contract contract) =>
        contract.GameContext.GetObject(GameContextObjectTagEnum.TargetStarSystem) as StarSystem;

    private static ContractTravel CreateTravel(StarSystem system, int? days) =>
        new(DefinitionReferences.ReferenceTo(system.Def.Description), SystemTags.ReadVisibleTags(system), days);

    // Mirrors the route days of SGContractsWidget.PopulateContract. The game's Starmap.FindRouteTo steps the path
    // finder over several frames; this steps an own one to the end, which only computes.
    private static int? ReadTravelDays(SimGameState simGame, StarSystem system)
    {
        var starmap = simGame.Starmap;
        AStar.AStarResult? route = null;
        var pathFinder = new AStar.PathFinder();
        pathFinder.InitFindPath(
            starmap.GetSystemByID(simGame.CurSystem.ID),
            starmap.GetSystemByID(system.ID),
            1,
            1E-06f,
            result => route = result);
        while (pathFinder.Step())
        {
        }

        if (route is not { status: PathStatus.Complete })
        {
            ModLog.Logger.LogWarning($"Found no route to {system.ID}; its travel days are left out");
            return null;
        }

        var nodes = route.path.Cast<StarSystemNode>().ToList();
        return starmap.DistanceToJumpship()
               + nodes.Take(nodes.Count - 1).Sum(node => node.Cost)
               + nodes[nodes.Count - 1].System.JumpDistance;
    }
}
