namespace FairwayQuest.Tests.Support;

using System;
using FairwayQuest.Core.Abstractions;

internal sealed class SeedRandomProvider : IRandomProvider
{
    private readonly System.Random random;

    public SeedRandomProvider(int seed)
    {
        random = new System.Random(seed);
    }

    public double NextDouble() => random.NextDouble();

    public double NextDouble(double minInclusive, double maxInclusive)
    {
        if (Math.Abs(maxInclusive - minInclusive) < double.Epsilon)
        {
            return minInclusive;
        }

        return minInclusive + ((maxInclusive - minInclusive) * random.NextDouble());
    }

    public int NextInt(int minInclusive, int maxExclusive) => random.Next(minInclusive, maxExclusive);
}
