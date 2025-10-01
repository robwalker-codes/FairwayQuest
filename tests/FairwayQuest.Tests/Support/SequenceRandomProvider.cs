namespace FairwayQuest.Tests.Support;

using System;
using System.Collections.Generic;
using FairwayQuest.Core.Abstractions;

internal sealed class SequenceRandomProvider : IRandomProvider
{
    private readonly Queue<double> sequence;

    public SequenceRandomProvider(IEnumerable<double> values)
    {
        sequence = new Queue<double>(values);
    }

    public double NextDouble()
    {
        if (sequence.Count == 0)
        {
            throw new InvalidOperationException("No random values remaining in sequence.");
        }

        return sequence.Dequeue();
    }

    public double NextDouble(double minInclusive, double maxInclusive)
    {
        var next = NextDouble();
        return minInclusive + ((maxInclusive - minInclusive) * next);
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        var span = maxExclusive - minInclusive;
        if (span <= 0)
        {
            return minInclusive;
        }

        var value = NextDouble();
        var scaled = (int)Math.Floor(value * span);
        return minInclusive + Math.Clamp(scaled, 0, span - 1);
    }
}
