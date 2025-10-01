using System;
using System.Collections.Generic;
using System.Linq;
using FairwayQuest.Core.Courses;
using FairwayQuest.Core.Scoring;

namespace FairwayQuest.Tests.Scoring;

public class StablefordAllocationTests
{
    private static readonly IReadOnlyList<FairwayQuest.Core.Models.Hole> NineHoleCourse = DefaultCourseProvider.CreateCourse(9);

    [Fact]
    public void BugRegression_StablefordAllocationsYieldFivePoints()
    {
        var strokeIndexes = NineHoleCourse.Select(h => h.StrokeIndex).ToArray();
        var effective = Handicap.ComputeEffectiveHandicap(22, 9, true);
        effective.Should().Be(10);

        var allocations = Handicap.AllocateStrokesPerHole(effective, strokeIndexes);
        var hardestHoleIndex = Array.FindIndex(strokeIndexes, si => si == 1);
        var hardestHole = NineHoleCourse[hardestHoleIndex];

        var net = ScoreCalculator.CalculateNetStrokes(3, allocations[hardestHoleIndex]);
        net.Should().Be(1);

        var relative = net - hardestHole.Par;
        var points = ScoreCalculator.StablefordPointsFromRelativeScore(relative);
        points.Should().Be(5);
    }

    [Fact]
    public void SameHoleWithSingleStrokeAwardsFourPoints()
    {
        var strokeIndexes = NineHoleCourse.Select(h => h.StrokeIndex).ToArray();
        var allocations = Handicap.AllocateStrokesPerHole(9, strokeIndexes);
        var hardestHoleIndex = Array.FindIndex(strokeIndexes, si => si == 1);
        var hardestHole = NineHoleCourse[hardestHoleIndex];

        var net = ScoreCalculator.CalculateNetStrokes(3, allocations[hardestHoleIndex]);
        net.Should().Be(2);

        var relative = net - hardestHole.Par;
        var points = ScoreCalculator.StablefordPointsFromRelativeScore(relative);
        points.Should().Be(4);
    }

    [Fact]
    public void SameHoleWithNoStrokesAwardsThreePoints()
    {
        var strokeIndexes = NineHoleCourse.Select(h => h.StrokeIndex).ToArray();
        var allocations = Handicap.AllocateStrokesPerHole(0, strokeIndexes);
        var hardestHoleIndex = Array.FindIndex(strokeIndexes, si => si == 1);
        var hardestHole = NineHoleCourse[hardestHoleIndex];

        var net = ScoreCalculator.CalculateNetStrokes(3, allocations[hardestHoleIndex]);
        net.Should().Be(3);

        var relative = net - hardestHole.Par;
        var points = ScoreCalculator.StablefordPointsFromRelativeScore(relative);
        points.Should().Be(3);
    }

    [Fact]
    public void NetScoreIsClampedToAtLeastOne()
    {
        var net = ScoreCalculator.CalculateNetStrokes(1, 5);
        net.Should().Be(1);
    }
}
