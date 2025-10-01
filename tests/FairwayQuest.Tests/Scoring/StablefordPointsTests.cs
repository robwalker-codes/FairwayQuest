using FairwayQuest.Core.Scoring;

namespace FairwayQuest.Tests.Scoring;

public class StablefordPointsTests
{
    [Theory]
    [InlineData(2, 0)]
    [InlineData(1, 1)]
    [InlineData(0, 2)]
    [InlineData(-1, 3)]
    [InlineData(-2, 4)]
    [InlineData(-3, 5)]
    [InlineData(-4, 6)]
    [InlineData(-5, 6)]
    public void MapsRelativeScoreToStablefordPoints(int relative, int expected)
    {
        var result = ScoreCalculator.StablefordPointsFromRelativeScore(relative);
        result.Should().Be(expected);
    }
}
