using FairwayQuest.Core.Models;
using FairwayQuest.Core.Simulation;

namespace FairwayQuest.Tests.Simulation;

public class ShotEngineTests
{
    [Fact]
    public void ShotsFromRoughTravelLessThanFairway()
    {
        var engine = new ShotEngine();
        var commonSequence = new[] { 0.5, 0.5, 0.6 };

        var fairwayOutcome = engine.ExecuteShot(new ShotState(160, Lie.Fairway), "7i", new FakeRandomNumberGenerator(commonSequence));
        var roughOutcome = engine.ExecuteShot(new ShotState(160, Lie.Rough), "7i", new FakeRandomNumberGenerator(commonSequence));

        roughOutcome.NewState.RemainingYards.Should().BeGreaterThan(fairwayOutcome.NewState.RemainingYards);
    }

    [Fact]
    public void ShortPuttsCanFinishInSinglePutt()
    {
        var engine = new ShotEngine();
        var outcome = engine.ExecuteShot(new ShotState(2, Lie.Green), "p", new FakeRandomNumberGenerator(new[] { 0.1 }));
        outcome.StrokesUsed.Should().Be(1);
        outcome.Holed.Should().BeTrue();
    }

    [Fact]
    public void ShortPuttsMayRequireTwoPutts()
    {
        var engine = new ShotEngine();
        var outcome = engine.ExecuteShot(new ShotState(2, Lie.Green), "p", new FakeRandomNumberGenerator(new[] { 0.95 }));
        outcome.StrokesUsed.Should().Be(2);
    }

    [Fact]
    public void LongPuttsCanTakeThreeStrokes()
    {
        var engine = new ShotEngine();
        var outcome = engine.ExecuteShot(new ShotState(12, Lie.Green), "p", new FakeRandomNumberGenerator(new[] { 0.95 }));
        outcome.StrokesUsed.Should().Be(3);
    }

    [Fact]
    public void SeededRandomProducesDeterministicOutcomes()
    {
        var engine = new ShotEngine();
        var seed = 424242;
        var clubs = new[] { "d", "7i", "sw", "p" };

        var rngOne = new RandomNumberGeneratorAdapter(new Random(seed));
        var rngTwo = new RandomNumberGeneratorAdapter(new Random(seed));

        var stateOne = ShotState.ForHoleStart(420);
        var stateTwo = ShotState.ForHoleStart(420);

        foreach (var club in clubs)
        {
            var outcomeOne = engine.ExecuteShot(stateOne, club, rngOne);
            var outcomeTwo = engine.ExecuteShot(stateTwo, club, rngTwo);

            outcomeOne.Description.Should().Be(outcomeTwo.Description);
            outcomeOne.NewState.Should().Be(outcomeTwo.NewState);
            outcomeOne.StrokesUsed.Should().Be(outcomeTwo.StrokesUsed);

            stateOne = outcomeOne.NewState;
            stateTwo = outcomeTwo.NewState;

            if (outcomeOne.Holed)
            {
                break;
            }
        }
    }
}
