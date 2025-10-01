namespace FairwayQuest.Core.Simulation;

public class RandomNumberGeneratorAdapter : IRandomNumberGenerator
{
    private readonly Random _random;

    public RandomNumberGeneratorAdapter(int seed)
    {
        _random = new Random(seed);
    }

    public RandomNumberGeneratorAdapter(Random random)
    {
        _random = random;
    }

    public double NextDouble() => _random.NextDouble();

    public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
}
