namespace FairwayQuest.Core.Handicapping;

/// <summary>
/// Calculates effective playing handicaps based on competition rules.
/// </summary>
public static class EffectiveHandicapCalculator
{
    /// <summary>
    /// Calculates the effective playing handicap for the specified format.
    /// </summary>
    /// <param name="handicapIndex">The player's base handicap index.</param>
    /// <param name="isStableford">Indicates whether Stableford allowances apply.</param>
    /// <param name="holesToPlay">The number of holes being played (9 or 18).</param>
    /// <returns>The effective playing handicap rounded down to the nearest integer.</returns>
    public static int ComputeEffectivePlayingHandicap(int handicapIndex, bool isStableford, int holesToPlay)
    {
        if (handicapIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(handicapIndex), handicapIndex, "Handicap must be non-negative.");
        }

        if (holesToPlay is not 9 and not 18)
        {
            throw new ArgumentOutOfRangeException(nameof(holesToPlay), holesToPlay, "Holes must be 9 or 18.");
        }

        var baseValue = (double)handicapIndex;
        if (isStableford)
        {
            baseValue *= 0.95;
        }

        if (holesToPlay == 9)
        {
            baseValue /= 2.0;
        }

        return (int)Math.Floor(baseValue);
    }
}
