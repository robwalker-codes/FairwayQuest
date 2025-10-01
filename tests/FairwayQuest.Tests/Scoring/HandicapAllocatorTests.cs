using System.Collections.Generic;
using System.Linq;
using FairwayQuest.Core.Courses;
using FairwayQuest.Core.Scoring;

namespace FairwayQuest.Tests.Scoring;

public class HandicapAllocatorTests
{
    private static readonly IReadOnlyList<Core.Models.Hole> NineHoleCourse = DefaultCourseProvider.CreateCourse(9);
    private static readonly IReadOnlyList<Core.Models.Hole> EighteenHoleCourse = DefaultCourseProvider.CreateCourse(18);

    public static IEnumerable<object[]> NineHoleCases() => new List<object[]>
    {
        new object[] { 0, Enumerable.Repeat(0, 9).ToArray() },
        new object[] { 9, Enumerable.Repeat(1, 9).ToArray() },
        new object[] { 18, Enumerable.Repeat(2, 9).ToArray() },
        new object[] { 36, Enumerable.Repeat(4, 9).ToArray() },
        new object[] { 54, Enumerable.Repeat(6, 9).ToArray() }
    };

    public static IEnumerable<object[]> EighteenHoleCases()
    {
        yield return new object[] { 0, Enumerable.Repeat(0, 18).ToArray() };
        yield return new object[] { 18, Enumerable.Repeat(1, 18).ToArray() };

        var twenty = Enumerable.Repeat(1, 18).ToArray();
        twenty[3]++;
        twenty[16]++;
        yield return new object[] { 20, twenty };

        yield return new object[] { 36, Enumerable.Repeat(2, 18).ToArray() };
        yield return new object[] { 54, Enumerable.Repeat(3, 18).ToArray() };
    }

    [Theory]
    [MemberData(nameof(NineHoleCases))]
    public void AllocatesExpectedStrokes_ForNineHoles(int handicap, int[] expected)
    {
        var result = HandicapAllocator.Allocate(handicap, NineHoleCourse);
        result.Should().Equal(expected);
    }

    [Theory]
    [MemberData(nameof(EighteenHoleCases))]
    public void AllocatesExpectedStrokes_ForEighteenHoles(int handicap, int[] expected)
    {
        var result = HandicapAllocator.Allocate(handicap, EighteenHoleCourse);
        result.Should().Equal(expected);
    }
}
