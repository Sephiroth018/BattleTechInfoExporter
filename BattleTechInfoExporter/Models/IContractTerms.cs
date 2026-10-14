namespace BattleTechInfoExporter.Models;

/// <summary>
///     What a contract gives on success: the terms it is accepted with, fixed or negotiated. A slider row
///     (<see cref="ValuesAtShare" />) isn't a deal, so it doesn't play this role.
/// </summary>
internal interface IContractTerms
{
    /// <summary>The pay in C-Bills.</summary>
    int Pay { get; }

    /// <summary>The salvage the company gets.</summary>
    Salvage Salvage { get; }

    /// <summary>The reputation changes when the contract succeeds.</summary>
    ReputationChange Reputation { get; }
}
