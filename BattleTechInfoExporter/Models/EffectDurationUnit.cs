namespace BattleTechInfoExporter.Models;

/// <summary>What an effect's duration counts, as <c>Effect</c> picks its <c>ETimer</c>.</summary>
internal enum EffectDurationUnit
{
    /// <summary>The activations of the unit that made the effect.</summary>
    Activations,

    /// <summary>The activations of the unit the effect is on.</summary>
    TargetActivations,
    Rounds,
    Movements,
    Phases
}
