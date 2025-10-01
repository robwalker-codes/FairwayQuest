namespace FairwayQuest.Core.Scoring;

/// <summary>
/// Performs scoring calculations for supported game formats.
/// </summary>
public static class ScoreCalculator
{
    private const int StablefordMinimum = 0;
    private const int StablefordMaximum = 6;

    /// <summary>
    /// Calculates Stableford points for a single hole.
    /// </summary>
    /// <param name="par">The hole par.</param>
    /// <param name="grossStrokes">The number of strokes actually taken.</param>
    /// <param name="strokesReceived">The strokes allocated via handicap.</param>
    /// <returns>A <see cref="HoleScore"/> describing the result.</returns>
    public static HoleScore CalculateStableford(int par, int grossStrokes, int strokesReceived)
    {
        if (par < 3)
        {
            throw new ArgumentOutOfRangeException(nameof(par), par, "Par must be at least 3.");
        }

        if (grossStrokes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(grossStrokes), grossStrokes, "Gross strokes must be positive.");
        }

        if (strokesReceived < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(strokesReceived), strokesReceived, "Strokes received cannot be negative.");
        }

        var netStrokes = grossStrokes - strokesReceived;
        var relativeToPar = netStrokes - par;
        var points = relativeToPar switch
        {
            <= -4 => 6,
            -3 => 5,
            -2 => 4,
            -1 => 3,
            0 => 2,
            1 => 1,
            _ => 0,
        };

        points = Math.Clamp(points, StablefordMinimum, StablefordMaximum);
        return new HoleScore(grossStrokes, netStrokes, points);
    }
}
