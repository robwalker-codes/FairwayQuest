using FairwayQuest.Core.Scoring;

namespace FairwayQuest.Tests.Scoring;

public class HandicapTests
{
    [Fact]
    public void UsesFullIndexForEighteenHoles()
    {
        var result = Handicap.ComputeEffectiveHandicap(22, 18, false);
        result.Should().Be(22);
    }

    [Fact]
    public void HalvesAndRoundsForNineHoleStrokePlay()
    {
        var result = Handicap.ComputeEffectiveHandicap(22, 9, false);
        result.Should().Be(11);
    }

    [Fact]
    public void AppliesStablefordAllowanceForNineHoles()
    {
        var result = Handicap.ComputeEffectiveHandicap(22, 9, true);
        result.Should().Be(10);
    }

    [Theory]
    [InlineData(21, 10)]
    [InlineData(23, 12)]
    public void RoundsHalvesAwayFromZeroWhenHalving(int handicapIndex18, int expected)
    {
        var result = Handicap.ComputeEffectiveHandicap(handicapIndex18, 9, false);
        result.Should().Be(expected);
    }

    [Fact]
    public void StablefordAllowanceRoundsAwayFromZero()
    {
        var result = Handicap.ComputeEffectiveHandicap(21, 9, true, 95);
        result.Should().Be(10);
    }
}
