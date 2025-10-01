namespace FairwayQuest.Tests.Courses;

using System.Linq;
using System.Threading.Tasks;
using FairwayQuest.Core.Allocation;
using FairwayQuest.Core.Models;
using FairwayQuest.Courses;
using FairwayQuest.Tests.TestData;
using FluentAssertions;
using Xunit;

public static class JsonCourseTests
{
    [Fact]
    public static async Task Load_PebbleBeach_ParSiAndYardsParse()
    {
        var repository = new JsonCourseRepository(TestPaths.CoursesDirectory);
        var course = (await repository.GetCoursesAsync().ConfigureAwait(false))
            .Single(c => c.Name == "Pebble Beach Golf Links");

        var hole6 = course.Holes.Single(h => h.Number == 6);
        hole6.Par.Should().Be(5);
        hole6.StrokeIndex.Men.Should().Be(2);
        hole6.Yards.Should().ContainKey("championship").WhoseValue.Should().Be(523);
    }

    [Fact]
    public static async Task Load_StAndrews_ParSiAndYardsParse()
    {
        var repository = new JsonCourseRepository(TestPaths.CoursesDirectory);
        var course = (await repository.GetCoursesAsync().ConfigureAwait(false))
            .Single(c => c.Name == "St Andrews - Old Course");

        var hole14 = course.Holes.Single(h => h.Number == 14);
        hole14.Par.Should().Be(5);
        hole14.StrokeIndex.Men.Should().Be(1);
        hole14.Yards.Should().ContainKey("blue").WhoseValue.Should().Be(523);
    }

    [Fact]
    public static async Task Select_FrontNine_UsesFrontNineSIs()
    {
        var course = await LoadStAndrewsAsync().ConfigureAwait(false);
        var holes = course.SelectHoles(HoleSelection.Front9);
        var mapped = StrokeIndexMapper.MapMenStrokeIndexes(holes);

        var indexOfHoleFive = Enumerable.Range(0, holes.Count).First(i => holes[i].Number == 5);
        mapped.Distinct().Should().HaveCount(holes.Count);
        mapped[indexOfHoleFive].Should().Be(1);
    }

    [Fact]
    public static async Task Select_BackNine_UsesBackNineSIs()
    {
        var course = await LoadStAndrewsAsync().ConfigureAwait(false);
        var holes = course.SelectHoles(HoleSelection.Back9);
        var mapped = StrokeIndexMapper.MapMenStrokeIndexes(holes);

        var indexOfHoleFourteen = Enumerable.Range(0, holes.Count).First(i => holes[i].Number == 14);
        mapped.Distinct().Should().HaveCount(holes.Count);
        mapped[indexOfHoleFourteen].Should().Be(1);
    }

    private static async Task<Course> LoadStAndrewsAsync()
    {
        var repository = new JsonCourseRepository(TestPaths.CoursesDirectory);
        return (await repository.GetCoursesAsync().ConfigureAwait(false))
            .Single(c => c.Name == "St Andrews - Old Course");
    }
}
