namespace FairwayQuest.Tests.Round;

using System;
using System.Collections.Generic;
using System.Linq;
using FairwayQuest.Cli;
using FairwayQuest.Cli.Round;
using FairwayQuest.Core.Gameplay;
using FairwayQuest.Core.Models;
using FairwayQuest.Core.Scoring;
using FairwayQuest.Core.Shots;
using FairwayQuest.Tests.Support;
using FluentAssertions;
using Xunit;

public sealed class RoundSimulationTests
{
    [Fact]
    public void EndToEndSingleHole()
    {
        var hole = CreateHole(1, 4, 360, 360);
        var player = new Player("Solo", 12, "Blue", 10, new[] { 1 });
        var state = new PlayerHoleState(360);
        var engine = new ShotEngine();
        var rng = new SeedRandomProvider(123);

        foreach (var club in new[] { "d", "7i", "pw", "p" })
        {
            var result = engine.Execute(new ShotRequest(state.Lie, state.RemainingYards, club), rng);
            state.ApplyShot(result);
        }

        state.IsHoled.Should().BeTrue();
        var gross = state.GrossStrokes;
        var score = ScoreCalculator.CalculateStableford(hole.Par, gross, player.AllocatedStrokesPerHole[0]);
        score.StablefordPoints.Should().BeGreaterThan(0);
    }

    [Fact]
    public void MultiPlayerTurnOrder()
    {
        var course = CreateCourseWithUniformYards(50, 50);
        var hole = course.Holes[0];
        var players = new List<Player>
        {
            new Player("A", 5, "Blue", 0, new[] { 0 }),
            new Player("B", 7, "Blue", 0, new[] { 0 }),
        };

        var round = new Round(course, new[] { hole }, players, GameFormat.StrokePlay);
        var selector = new RecordingShotSelector(new[] { "pw", "pw", "p", "p" });
        var rng = new SequenceRandomProvider(new[]
        {
            0.5, 0.1, 0.05, 0.5, 0.2, 0.4,
            0.5, 0.1, 0.05, 0.5, 0.2, 0.4,
            0.2,
            0.2,
        });

        var runner = new RoundRunner(round, new ShotEngine(), rng, new AppOptions(), shotSelector: selector);
        var trackers = runner.PlayRound();

        selector.Calls.Select(call => call.Player).Should().Equal("A", "B", "A", "B");
        trackers.Select(t => t.GrossPerHole.Single()).Should().OnlyContain(strokes => strokes > 0);
    }

    [Fact]
    public void CourseSelectionHonoursPlayerTeeYardage()
    {
        var course = CreateCourseWithUniformYards(300, 280);
        var hole = course.Holes[0];
        var players = new List<Player>
        {
            new Player("BluePlayer", 10, "Blue", 0, new[] { 0 }),
            new Player("WhitePlayer", 10, "White", 0, new[] { 0 }),
        };

        var selector = new RecordingShotSelector(new[] { "p", "p" });
        var rng = new SequenceRandomProvider(new[] { 0.1, 0.1 });
        var round = new Round(course, new[] { hole }, players, GameFormat.StrokePlay);
        var runner = new RoundRunner(round, new ShotEngine(), rng, new AppOptions(), shotSelector: selector);
        runner.PlayRound();

        selector.Calls.Should().HaveCount(2);
        selector.Calls[0].Remaining.Should().Be(300);
        selector.Calls[1].Remaining.Should().Be(280);
    }

    private static Course CreateCourseWithUniformYards(int blue, int white)
    {
        var holes = Enumerable.Range(1, 9)
            .Select(i => CreateHole(i, 4, blue, white))
            .ToList();

        var tees = new Dictionary<string, TeeMetadata>(StringComparer.OrdinalIgnoreCase)
        {
            ["Blue"] = new TeeMetadata(70, 120),
            ["White"] = new TeeMetadata(69, 115),
        };

        return new Course("Test Course", "Testville", holes, "Blue", tees);
    }

    private static Hole CreateHole(int number, int par, int blueYards, int whiteYards)
    {
        var yards = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Blue"] = blueYards,
            ["White"] = whiteYards,
        };

        return new Hole(number, par, new StrokeIndexSet(number, number), yards);
    }
}
