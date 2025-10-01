namespace FairwayQuest.Core.Abstractions;

/// <summary>
/// Provides deterministic random values for the shot engine and simulations.
/// </summary>
public interface IRandomProvider
{
    /// <summary>
    /// Gets the next double in the interval [0, 1).
    /// </summary>
    /// <returns>The sampled value.</returns>
    double NextDouble();

    /// <summary>
    /// Gets the next double constrained to the provided range.
    /// </summary>
    /// <param name="minInclusive">The inclusive lower bound.</param>
    /// <param name="maxInclusive">The inclusive upper bound.</param>
    /// <returns>The sampled value within the range.</returns>
    double NextDouble(double minInclusive, double maxInclusive);

    /// <summary>
    /// Gets the next integer within the specified range.
    /// </summary>
    /// <param name="minInclusive">The inclusive lower bound.</param>
    /// <param name="maxExclusive">The exclusive upper bound.</param>
    /// <returns>The sampled integer.</returns>
    int NextInt(int minInclusive, int maxExclusive);
}
