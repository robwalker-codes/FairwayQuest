namespace FairwayQuest.Tests.Allocation;

using FairwayQuest.Core.Allocation;
using FairwayQuest.Tests.TestData;
using FluentAssertions;
using Xunit;

public static class StrokeAllocationTests
{
    [Fact]
    public static void NineHoleCourse_HandicapLessThanHoleCount_AllocatesSingleStrokes()
    {
        var course = TestCourses.CreateNineHoleCourse();
        var holes = course.SelectHoles(Core.Models.HoleSelection.Front9);
        var strokeIndexes = StrokeIndexMapper.MapMenStrokeIndexes(holes);
        var allocation = StrokeAllocator.Allocate(4, strokeIndexes);

        allocation.Should().Equal(1, 1, 1, 1, 0, 0, 0, 0, 0);
    }

    [Fact]
    public static void NineHoleCourse_HandicapExceedsHoleCount_AddsSecondPass()
    {
        var course = TestCourses.CreateNineHoleCourse();
        var holes = course.SelectHoles(Core.Models.HoleSelection.Front9);
        var strokeIndexes = StrokeIndexMapper.MapMenStrokeIndexes(holes);
        var allocation = StrokeAllocator.Allocate(12, strokeIndexes);

        allocation.Should().Equal(2, 2, 2, 1, 1, 1, 1, 1, 1);
    }
}
