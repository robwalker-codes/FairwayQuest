namespace FairwayQuest.Tests.Scoring;

using System.Linq;
using FairwayQuest.Core.Allocation;
using FairwayQuest.Core.Models;
using FairwayQuest.Core.Scoring;
using FairwayQuest.Tests.TestData;
using FluentAssertions;
using Xunit;

public static class StablefordScoringTests
{
    [Fact]
    public static void Par5_HardestHole_Gross3_WithTwoStrokes_YieldsSixPoints()
    {
        var course = TestCourses.CreateStablefordRegressionCourse();
        var holes = course.SelectHoles(HoleSelection.Back9);
        var strokeIndexes = StrokeIndexMapper.MapMenStrokeIndexes(holes);
        var allocation = StrokeAllocator.Allocate(10, strokeIndexes);

        var targetIndex = Enumerable.Range(0, holes.Count).First(i => holes[i].Number == 14);
        var hole = holes[targetIndex];
        var strokesReceived = allocation[targetIndex];

        var result = ScoreCalculator.CalculateStableford(hole.Par, 3, strokesReceived);
        result.StablefordPoints.Should().Be(6);
    }

    [Fact]
    public static void SameHole_NoStrokes_ParIsTwoPoints()
    {
        var hole = TestCourses.CreateStablefordRegressionCourse().SelectHoles(HoleSelection.Back9)
            .Single(h => h.Number == 14);

        var result = ScoreCalculator.CalculateStableford(hole.Par, hole.Par, 0);
        result.StablefordPoints.Should().Be(2);
    }

    [Fact]
    public static void SameHole_SingleStroke_BirdieIsFourPoints()
    {
        var hole = TestCourses.CreateStablefordRegressionCourse().SelectHoles(HoleSelection.Back9)
            .Single(h => h.Number == 14);

        var result = ScoreCalculator.CalculateStableford(hole.Par, 4, 1);
        result.StablefordPoints.Should().Be(4);
    }
}
