namespace FairwayQuest.Core.Models;

public sealed class Player
{
    public required string Name { get; init; }

    public required int HandicapIndex18 { get; init; }

    public required int EffectiveHandicap { get; init; }

    public required int[] AllocatedStrokesPerHole { get; init; }
}
