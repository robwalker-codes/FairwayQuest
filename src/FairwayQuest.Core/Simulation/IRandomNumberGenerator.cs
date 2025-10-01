namespace FairwayQuest.Core.Simulation;

public interface IRandomNumberGenerator
{
    double NextDouble();
    int Next(int minInclusive, int maxExclusive);
}
