namespace FairwayQuest.Core.Models;

public class Player
{
    public required string Name { get; init; }

    public required int Handicap { get; init; }

    public int[] AllocatedStrokesPerHole { get; private set; } = Array.Empty<int>();

    public void SetAllocatedStrokes(int[] strokes)
    {
        ArgumentNullException.ThrowIfNull(strokes);
        AllocatedStrokesPerHole = strokes;
    }
}
