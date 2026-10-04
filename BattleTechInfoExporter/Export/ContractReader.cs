using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Framework;
using BattleTechInfoExporter.Models;
using HBS.Nav;
using UnityEngine;
using Contract = BattleTechInfoExporter.Models.Contract;

namespace BattleTechInfoExporter.Export;

/// <summary>Builds the Command Center's contracts and the mission type table from the game's career state.</summary>
internal static class ContractReader
{
    // The slider positions of the contract negotiation in percent; their step is set in the screen's UI asset.
    private static readonly int[] NegotiationShares = [0, 25, 50, 75, 100];

    // The game generates a system's contracts when the contract screen first opens there
    // (SGRoomController_CmdCenter.StartContractScreen). The mod never generates them itself: that would change the
    // career.
    internal static List<Contract>? ReadContracts(SimGameState simGame) =>
        simGame.CurSystem.InitialContractsFetched
            ? simGame.GetAllCurrentlySelectableContracts().Select(contract => ReadContract(simGame, contract)).ToList()
            : null;

    // SimGameState.ContractTypeDescriptions has the procedural mission types; priority contracts share one entry.
    internal static List<ContractTypeDescription> ReadContractTypes(SimGameState simGame)
    {
        var contractTypes = simGame.ContractTypeDescriptions
            .Select(description => new ContractTypeDescription(
                ReferenceTo(ContractTypeEnumeration.GetContractTypeByInt(description.Key)),
                description.Value.Details))
            .ToList();
        if (simGame.PriorityMissionDescription is { } priority)
        {
            contractTypes.Add(new ContractTypeDescription(PriorityType(simGame), priority.Details));
        }

        return contractTypes;
    }

    private static Contract ReadContract(SimGameState simGame, BattleTech.Contract contract)
    {
        var contractOverride = contract.Override;
        var employer = contract.GetTeamFaction(contractOverride.employerTeam.teamGuid);
        var target = contract.GetTeamFaction(contractOverride.targetTeam.teamGuid);
        return new Contract(
            ReadId(contractOverride),
            // The contract list and details show the raw name; Contract.Name interpolates it, which the game's
            // contracts don't need.
            contractOverride.contractName,
            // Mirrors Contract.GetContractTypeString and the type tooltip of SGContractsWidget.PopulateContract.
            contract.IsPriorityContract ? PriorityType(simGame) : ReferenceTo(contractOverride.ContractTypeValue),
            contractOverride.contractDisplayStyle,
            DefinitionReferences.ReferenceTo(employer),
            DefinitionReferences.ReferenceTo(target),
            // Mirrors SimGameState.ContractUserMeetsReputation_Campaign.
            Mathf.Min(
                contractOverride.finalDifficulty + contractOverride.difficultyUIModifier,
                (int)simGame.Constants.Story.GlobalContractDifficultyMax),
            simGame.ContractUserMeetsReputation(contract),
            ReadNegotiation(simGame, contract, employer, target),
            ReadLanceLimits(contractOverride),
            ReadBiome(simGame, contract.ContractBiome),
            ReadTravel(simGame, contract));
    }

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
                [Option(null, null, contract.Override.negotiatedSalary, contract.Override.negotiatedSalvage)]);
        }

        // The sliders' shares can't exceed 100 together; without employer reputation they are coupled and leave
        // nothing for it (SGContractsWidget.OnNegPaymentChange, ShouldAdjustReputation). Accepting sets the
        // reputation share to the rest (Contract.SetNegotiatedValues).
        return new Negotiation(
            true,
            NegotiationShares
                .SelectMany(pay => NegotiationShares.Select(salvage => (pay, salvage)))
                .Where(shares => employer.DoesGainReputation
                    ? shares.pay + shares.salvage <= 100
                    : shares.pay + shares.salvage == 100)
                .Select(shares => Option(shares.pay, shares.salvage, shares.pay / 100f, shares.salvage / 100f))
                .ToList());

        NegotiationOption Option(int? payPercent, int? salvagePercent, float payShare, float salvageShare) =>
            new(
                payPercent,
                salvagePercent,
                ReadPay(simGame, contract, payShare),
                ReadSalvage(simGame, contract, salvageShare),
                ReadReputation(simGame, contract, employer, target, 1f - (payShare + salvageShare)));
    }

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
        var mechs = Enumerable.Range(0,
                Math.Min(contractOverride.maxNumberOfPlayerUnits, contractOverride.mechMinTonnages.Length))
            .Select(slot => new MechSlotLimits(
                LimitOf(contractOverride.mechMinTonnages[slot]),
                LimitOf(contractOverride.mechMaxTonnages[slot])))
            .ToList();
        return new LanceLimits(
            contractOverride.maxNumberOfPlayerUnits,
            LimitOf(contractOverride.lanceMinTonnage),
            LimitOf(contractOverride.lanceMaxTonnage),
            mechs.Any(limits => limits.MinTonnage is not null || limits.MaxTonnage is not null) ? mechs : null);
    }

    private static float? LimitOf(float tonnage) => tonnage < 0 ? null : tonnage;

    private static DefinitionReference? ReadBiome(SimGameState simGame, Biome.BIOMESKIN biome) =>
        biome == Biome.BIOMESKIN.generic ? null : DefinitionReferences.ReferenceTo(simGame.DataManager, biome);

    // The contract list marks travel contracts by the target system in the contract's context (SGContractsListItem).
    private static ContractTravel? ReadTravel(SimGameState simGame, BattleTech.Contract contract)
    {
        if (contract.GameContext.GetObject(GameContextObjectTagEnum.TargetStarSystem) is not StarSystem system
            || system == simGame.CurSystem)
        {
            return null;
        }

        return new ContractTravel(
            DefinitionReferences.ReferenceTo(system.Def.Description),
            SystemTags.ReadVisibleTags(system),
            ReadTravelDays(simGame, system));
    }

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
