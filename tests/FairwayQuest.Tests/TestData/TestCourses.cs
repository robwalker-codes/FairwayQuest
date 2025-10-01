namespace FairwayQuest.Tests.TestData;

using System.Collections.Generic;
using System.Linq;
using FairwayQuest.Core.Models;

public static class TestCourses
{
    public static Course CreateNineHoleCourse()
    {
        var holes = Enumerable.Range(1, 9)
            .Select(i => CreateHole(i, par: 4, strokeIndex: i))
            .ToList();

        return CreateCourse("Test 9", holes);
    }

    public static Course CreateEighteenHoleCourse()
    {
        var holes = Enumerable.Range(1, 18)
            .Select(i => CreateHole(i, par: 4 + (i % 3 == 0 ? 1 : 0), strokeIndex: i))
            .ToList();

        return CreateCourse("Test 18", holes);
    }

    public static Course CreateStablefordRegressionCourse()
    {
        var holes = Enumerable.Range(1, 18)
            .Select(number => number switch
            {
                14 => CreateHole(number, par: 5, strokeIndex: 1),
                10 => CreateHole(number, par: 4, strokeIndex: 10),
                _ => CreateHole(number, par: 4, strokeIndex: number),
            })
            .ToList();

        return CreateCourse("Regression", holes);
    }

    private static Course CreateCourse(string name, IReadOnlyList<Hole> holes)
    {
        var tees = new Dictionary<string, TeeMetadata>
        {
            ["test"] = new(null, null),
        };

        return new Course(name, "Testville", holes, "test", tees);
    }

    private static Hole CreateHole(int number, int par, int strokeIndex)
    {
        var yards = new Dictionary<string, int>
        {
            ["test"] = 400,
        };

        return new Hole(number, par, new StrokeIndexSet(strokeIndex, null), yards);
    }
}
