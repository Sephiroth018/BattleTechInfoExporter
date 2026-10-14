using BattleTech;

namespace BattleTechInfoExporter.Models;

/// <summary>A value an event's requirement compares, as SimGameState.MeetsStatRequirements compares it.</summary>
internal interface IComparison
{
    Operator Operator { get; }
    float Value { get; }
}
