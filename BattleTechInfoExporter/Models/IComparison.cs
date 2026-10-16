using BattleTech;

namespace BattleTechInfoExporter.Models;

/// <summary>A value an event's requirement compares, as SimGameState.MeetsStatRequirements compares it.</summary>
internal interface IComparison<out TValue>
{
    Operator Operator { get; }
    TValue Value { get; }
}
