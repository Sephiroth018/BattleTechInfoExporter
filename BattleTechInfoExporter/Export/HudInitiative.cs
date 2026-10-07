namespace BattleTechInfoExporter.Export;

/// <summary>
///     Converts the game's phase and initiative, where units act from 1 up, into the HUD's numbering, where they act
///     from 5 down.
/// </summary>
internal static class HudInitiative
{
    // Mirrors AbstractActor.InitiativeToString, which every phase and initiative on the HUD goes through.
    internal static int FromGamePhase(int phase) => 6 - phase;
}
