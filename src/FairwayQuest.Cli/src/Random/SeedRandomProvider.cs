namespace FairwayQuest.Cli.Random;

using System.Diagnostics.CodeAnalysis;
using FairwayQuest.Core.Abstractions;

/// <summary>
/// Deterministic <see cref="System.Random"/> wrapper implementing <see cref="IRandomProvider"/>.
/// </summary>
[SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Deterministic gameplay is required for reproducible rounds.")]
internal sealed class SeedRandomProvider : IRandomProvider
{
    private readonly System.Random random;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedRandomProvider"/> class.
    /// </summary>
    /// <param name="seed">Optional deterministic seed.</param>
    public SeedRandomProvider(int? seed)
    {
        this.random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
    }

    /// <inheritdoc />
    public double NextDouble() => this.random.NextDouble();

    /// <inheritdoc />
    public double NextDouble(double minInclusive, double maxInclusive)
    {
        if (maxInclusive < minInclusive)
        {
            throw new ArgumentException("Max cannot be less than min.");
        }

        if (Math.Abs(maxInclusive - minInclusive) < double.Epsilon)
        {
            return minInclusive;
        }

        return minInclusive + ((maxInclusive - minInclusive) * this.random.NextDouble());
    }

    /// <inheritdoc />
    public int NextInt(int minInclusive, int maxExclusive) => this.random.Next(minInclusive, maxExclusive);
}
