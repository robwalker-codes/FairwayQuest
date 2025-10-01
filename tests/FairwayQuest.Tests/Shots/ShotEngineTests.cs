namespace FairwayQuest.Tests.Shots;

using FairwayQuest.Core.Shots;
using FairwayQuest.Tests.Support;
using FluentAssertions;
using Xunit;

public sealed class ShotEngineTests
{
    private readonly ShotEngine engine = new();

    [Fact]
    public void ShotEngineDeterminism()
    {
        var request = new ShotRequest(Lie.Fairway, 160, "7i");
        var first = engine.Execute(request, new SeedRandomProvider(123));
        var second = engine.Execute(request, new SeedRandomProvider(123));

        first.Lateral.Should().Be(second.Lateral);
        first.NewLie.Should().Be(second.NewLie);
        first.NewRemaining.Should().BeApproximately(second.NewRemaining, 0.01);
    }

    [Fact]
    public void LieModifiersApplied()
    {
        var sequence = new[] { 0.5, 0.1, 0.05, 0.5, 0.2, 0.4 };
        var fairway = engine.Execute(new ShotRequest(Lie.Fairway, 150, "7i"), new SequenceRandomProvider(sequence));
        var rough = engine.Execute(new ShotRequest(Lie.Rough, 150, "7i"), new SequenceRandomProvider(sequence));

        rough.Distance.Should().BeLessThan(fairway.Distance);
    }

    [Fact]
    public void PuttingBands_InsideTwoYardsPrefersSinglePutt()
    {
        var result = engine.Execute(new ShotRequest(Lie.Green, 1.5, "p"), new SequenceRandomProvider(new[] { 0.1 }));
        result.StrokeIncrement.Should().Be(1);
    }

    [Fact]
    public void PuttingBands_LongPuttsTakeMultipleStrokes()
    {
        var twoPutt = engine.Execute(new ShotRequest(Lie.Green, 10, "p"), new SequenceRandomProvider(new[] { 0.2 }));
        var threePutt = engine.Execute(new ShotRequest(Lie.Green, 10, "p"), new SequenceRandomProvider(new[] { 0.9 }));

        twoPutt.StrokeIncrement.Should().BeGreaterThanOrEqualTo(2);
        threePutt.StrokeIncrement.Should().BeGreaterThanOrEqualTo(2);
        threePutt.StrokeIncrement.Should().BeLessThanOrEqualTo(3);
    }

    [Fact]
    public void PenaltyHandling_ReturnsPenaltyStrokeAndNoProgress()
    {
        var result = engine.Execute(new ShotRequest(Lie.Tee, 360, "d"), new SequenceRandomProvider(new[] { 0.001 }));
        result.PenaltyStrokes.Should().Be(1);
        result.NewRemaining.Should().Be(360);
        result.Holed.Should().BeFalse();
    }
}
