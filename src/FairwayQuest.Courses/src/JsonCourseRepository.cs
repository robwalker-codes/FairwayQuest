namespace FairwayQuest.Courses;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FairwayQuest.Core.Abstractions;
using FairwayQuest.Core.Models;

/// <summary>
/// Discovers and loads course definitions from JSON documents on disk.
/// </summary>
public sealed class JsonCourseRepository : ICourseRepository
{
    private readonly string coursesDirectory;
    private readonly JsonSerializerOptions serializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonCourseRepository"/> class.
    /// </summary>
    /// <param name="coursesDirectory">The directory containing <c>*.course.json</c> files.</param>
    public JsonCourseRepository(string coursesDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(coursesDirectory);
        this.coursesDirectory = coursesDirectory;
        this.serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Course>> GetCoursesAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(this.coursesDirectory))
        {
            throw new DirectoryNotFoundException($"Course directory '{this.coursesDirectory}' was not found.");
        }

        var files = Directory.EnumerateFiles(this.coursesDirectory, "*.course.json", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var courses = new List<Course>(files.Count);
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var json = await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false);
            var document = JsonSerializer.Deserialize<CourseDocument>(json, this.serializerOptions)
                ?? throw new InvalidDataException($"File '{file}' does not contain a valid course definition.");

            courses.Add(ToCourse(document, file));
        }

        return new ReadOnlyCollection<Course>(courses);
    }

    private static Course ToCourse(CourseDocument document, string filePath)
    {
        if (document.Holes is null || document.Holes.Count != 18)
        {
            throw new InvalidDataException($"Course '{filePath}' must define exactly 18 holes.");
        }

        var holes = document.Holes
            .OrderBy(h => h.Number)
            .Select(hole => ToHole(hole, filePath))
            .ToList();

        ValidateHoles(holes, filePath);

        if (document.TeesMeta is null || document.TeesMeta.Count == 0)
        {
            throw new InvalidDataException($"Course '{filePath}' must define at least one tee set.");
        }

        if (string.IsNullOrWhiteSpace(document.DefaultTee))
        {
            throw new InvalidDataException($"Course '{filePath}' must specify a defaultTee value.");
        }

        var teeMetadata = document.TeesMeta.ToDictionary(
            pair => pair.Key,
            pair => new TeeMetadata(pair.Value?.RatingMen, pair.Value?.SlopeMen),
            StringComparer.OrdinalIgnoreCase);

        return new Course(
            document.Name ?? throw new InvalidDataException($"Course '{filePath}' is missing a name."),
            document.Location ?? throw new InvalidDataException($"Course '{filePath}' is missing a location."),
            holes,
            document.DefaultTee,
            teeMetadata);
    }

    private static Hole ToHole(HoleDocument hole, string filePath)
    {
        if (hole.Yards is null || hole.Yards.Count == 0)
        {
            throw new InvalidDataException($"Course '{filePath}' hole {hole.Number} must specify yardages.");
        }

        if (hole.Par <= 0)
        {
            throw new InvalidDataException($"Course '{filePath}' hole {hole.Number} is missing a valid par value.");
        }

        if (hole.StrokeIndexMen <= 0)
        {
            throw new InvalidDataException($"Course '{filePath}' hole {hole.Number} must specify a men's stroke index.");
        }

        return new Hole(
            hole.Number,
            hole.Par,
            new StrokeIndexSet(hole.StrokeIndexMen, hole.StrokeIndexWomen),
            hole.Yards);
    }

    private static void ValidateHoles(IReadOnlyList<Hole> holes, string filePath)
    {
        var numbers = holes.Select(h => h.Number).ToList();
        var expectedNumbers = Enumerable.Range(1, 18).ToList();
        if (!numbers.SequenceEqual(expectedNumbers))
        {
            throw new InvalidDataException($"Course '{filePath}' must provide holes numbered 1 through 18.");
        }

        var strokeIndexes = holes.Select(h => h.StrokeIndex.Men).OrderBy(i => i).ToList();
        var expectedIndexes = Enumerable.Range(1, 18).ToList();
        if (!strokeIndexes.SequenceEqual(expectedIndexes))
        {
            throw new InvalidDataException($"Course '{filePath}' must assign each men's stroke index from 1 to 18 exactly once.");
        }
    }

    private sealed class CourseDocument
    {
        public string? Name { get; set; }

        public string? Location { get; set; }

        public string? DefaultTee { get; set; }

        public List<HoleDocument>? Holes { get; set; }

        public Dictionary<string, TeeMetadataDocument?>? TeesMeta { get; set; }
    }

    private sealed class HoleDocument
    {
        public int Number { get; set; }

        public int Par { get; set; }

        public int StrokeIndexMen { get; set; }

        public int? StrokeIndexWomen { get; set; }

        public Dictionary<string, int>? Yards { get; set; }
    }

    private sealed class TeeMetadataDocument
    {
        public double? RatingMen { get; set; }

        public int? SlopeMen { get; set; }
    }
}
