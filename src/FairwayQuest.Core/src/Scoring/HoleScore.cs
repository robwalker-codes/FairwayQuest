namespace FairwayQuest.Core.Scoring;

/// <summary>
/// Represents the scoring outcome for a hole.
/// </summary>
public sealed class HoleScore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HoleScore"/> class.
    /// </summary>
    /// <param name="grossStrokes">The number of strokes actually taken.</param>
    /// <param name="netStrokes">The net strokes after handicap allowance.</param>
    /// <param name="stablefordPoints">Stableford points earned on the hole.</param>
    public HoleScore(int grossStrokes, int netStrokes, int stablefordPoints)
    {
        GrossStrokes = grossStrokes;
        NetStrokes = netStrokes;
        StablefordPoints = stablefordPoints;
    }

    /// <summary>
    /// Gets the number of strokes actually taken.
    /// </summary>
    public int GrossStrokes { get; }

    /// <summary>
    /// Gets the net strokes after handicap allowance.
    /// </summary>
    public int NetStrokes { get; }

    /// <summary>
    /// Gets the Stableford points earned on the hole.
    /// </summary>
    public int StablefordPoints { get; }
}
