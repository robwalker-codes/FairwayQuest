using System;
using System.Collections.Generic;
using FairwayQuest.Core.Simulation;

namespace FairwayQuest.Tests.Simulation;

public class FakeRandomNumberGenerator : IRandomNumberGenerator
{
    private readonly Queue<double> _doubleValues;
    private readonly Queue<int> _intValues;

    public FakeRandomNumberGenerator(IEnumerable<double>? doubles = null, IEnumerable<int>? ints = null)
    {
        _doubleValues = new Queue<double>(doubles ?? Array.Empty<double>());
        _intValues = new Queue<int>(ints ?? Array.Empty<int>());
    }

    public double NextDouble()
    {
        if (_doubleValues.Count == 0)
        {
            throw new InvalidOperationException("No more double values configured.");
        }

        return _doubleValues.Dequeue();
    }

    public int Next(int minInclusive, int maxExclusive)
    {
        if (_intValues.Count == 0)
        {
            throw new InvalidOperationException("No more int values configured.");
        }

        return _intValues.Dequeue();
    }
}
