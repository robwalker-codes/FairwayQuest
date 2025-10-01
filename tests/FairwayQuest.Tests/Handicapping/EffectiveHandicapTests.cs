namespace FairwayQuest.Tests.Handicapping;

using FairwayQuest.Core.Handicapping;
using FluentAssertions;
using Xunit;

public static class EffectiveHandicapTests
{
    [Fact]
    public static void Stableford_18H_EPH_RoundsDown()
    {
        var result = EffectiveHandicapCalculator.ComputeEffectivePlayingHandicap(22, true, 18);
        result.Should().Be(20);
    }

    [Fact]
    public static void Stableford_9H_EPH_RoundsDown()
    {
        var result = EffectiveHandicapCalculator.ComputeEffectivePlayingHandicap(22, true, 9);
        result.Should().Be(10);
    }
}
