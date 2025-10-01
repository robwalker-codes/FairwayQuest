using System;
using System.Collections.Generic;
using System.Linq;
using FairwayQuest.Core.Courses;
using FairwayQuest.Core.Scoring;

namespace FairwayQuest.Tests.Scoring;

public class StrokeAllocationTests
{
    private static readonly IReadOnlyList<FairwayQuest.Core.Models.Hole> NineHoleCourse = DefaultCourseProvider.CreateCourse(9);
    private static readonly IReadOnlyList<FairwayQuest.Core.Models.Hole> EighteenHoleCourse = DefaultCourseProvider.CreateCourse(18);

    [Fact]
    public void AllocatesExtraStrokesToLowestStrokeIndexes_ForNineHoles()
    {
        var strokeIndexes = NineHoleCourse.Select(h => h.StrokeIndex).ToArray();

        var result = Handicap.AllocateStrokesPerHole(11, strokeIndexes);

        result.Should().HaveCount(9);
        result.Count(value => value == 2).Should().Be(2);
        result.Count(value => value == 1).Should().Be(7);
        result.Sum().Should().Be(11);

        var hardestHoleIndex = Array.FindIndex(strokeIndexes, si => si == 1);
        var secondHardestIndex = Array.FindIndex(strokeIndexes, si => si == 2);
        result[hardestHoleIndex].Should().Be(2);
        result[secondHardestIndex].Should().Be(2);
    }

    [Fact]
    public void AllocatesExtrasToHardestHoles_ForEighteenHoles()
    {
        var strokeIndexes = EighteenHoleCourse.Select(h => h.StrokeIndex).ToArray();

        var result = Handicap.AllocateStrokesPerHole(20, strokeIndexes);

        result.Should().HaveCount(18);
        result.Sum().Should().Be(20);
        result.All(value => value == 1 || value == 2).Should().BeTrue();

        var hardestHoleIndex = Array.FindIndex(strokeIndexes, si => si == 1);
        var secondHardestIndex = Array.FindIndex(strokeIndexes, si => si == 2);
        result[hardestHoleIndex].Should().Be(2);
        result[secondHardestIndex].Should().Be(2);
        result.Count(value => value == 1).Should().Be(16);
    }
}
