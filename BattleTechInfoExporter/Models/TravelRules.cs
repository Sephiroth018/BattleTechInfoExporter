using JetBrains.Annotations;

namespace BattleTechInfoExporter.Models;

/// <summary>
///     How long a trip takes, which each star system's route in <see cref="Starmap.StarSystems" /> states: the
///     days of each jump plus the days to and from the jump point of the star systems at both ends, scaled by the
///     Argo's drive upgrades.
/// </summary>
/// <param name="DaysPerJump">Of a jump from a star system without a fuel station.</param>
/// <param name="DaysPerJumpFromFuelStation">Of a jump from a star system with one.</param>
/// <param name="DefaultDaysToJumpPoint">For a star system that sets no distance of its own.</param>
/// <param name="MaxJumpDistance">The farthest a jump reaches on the starmap, in light years.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record TravelRules(
    int DaysPerJump,
    int DaysPerJumpFromFuelStation,
    int DefaultDaysToJumpPoint,
    int MaxJumpDistance);
