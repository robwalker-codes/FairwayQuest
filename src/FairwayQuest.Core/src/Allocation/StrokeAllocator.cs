namespace FairwayQuest.Core.Allocation;

/// <summary>
/// Allocates handicap strokes to holes based on stroke index ordering.
/// </summary>
public static class StrokeAllocator
{
    /// <summary>
    /// Allocates handicap strokes on a per-hole basis.
    /// </summary>
    /// <param name="effectiveHandicap">The effective playing handicap.</param>
    /// <param name="strokeIndexes">The stroke indexes for the selected holes.</param>
    /// <returns>A list containing the strokes allocated to each hole.</returns>
    public static IReadOnlyList<int> Allocate(int effectiveHandicap, IReadOnlyList<int> strokeIndexes)
    {
        ArgumentNullException.ThrowIfNull(strokeIndexes);

        if (strokeIndexes.Count == 0)
        {
            throw new ArgumentException("Stroke indexes cannot be empty.", nameof(strokeIndexes));
        }

        if (effectiveHandicap < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveHandicap), effectiveHandicap, "Handicap cannot be negative.");
        }

        var holes = strokeIndexes.Count;
        var allocation = new int[holes];

        if (effectiveHandicap == 0)
        {
            return allocation;
        }

        var threshold = effectiveHandicap;
        while (threshold > 0)
        {
            for (var i = 0; i < holes; i++)
            {
                if (strokeIndexes[i] <= threshold)
                {
                    allocation[i] += 1;
                }
            }

            threshold -= holes;
        }

        return allocation;
    }
}
