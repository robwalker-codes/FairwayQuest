namespace FairwayQuest.Cli;

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FairwayQuest.Cli.Round;
using FairwayQuest.Core.Abstractions;
using FairwayQuest.Core.Allocation;
using FairwayQuest.Core.Gameplay;
using FairwayQuest.Core.Handicapping;
using FairwayQuest.Core.Models;
using FairwayQuest.Core.Scoring;
using FairwayQuest.Core.Shots;
using GameplayRound = FairwayQuest.Core.Gameplay.Round;

#pragma warning disable CA1303 // CLI intentionally writes literal strings to the console.

/// <summary>
/// Console application orchestrating interactive FairwayQuest rounds.
/// </summary>
internal sealed class Application
{
    private readonly ICourseRepository courseRepository;
    private readonly IRandomProvider randomProvider;
    private readonly AppOptions options;
    private readonly ShotEngine shotEngine = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Application"/> class.
    /// </summary>
    /// <param name="courseRepository">Course repository supplying available courses.</param>
    /// <param name="randomProvider">Random provider for deterministic simulations.</param>
    /// <param name="options">Command-line options.</param>
    public Application(ICourseRepository courseRepository, IRandomProvider randomProvider, AppOptions options)
    {
        this.courseRepository = courseRepository;
        this.randomProvider = randomProvider;
        this.options = options;
    }

    /// <summary>
    /// Executes the CLI workflow.
    /// </summary>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task RunAsync()
    {
        Console.WriteLine("FairwayQuest — Shot-by-Shot Edition");
        Console.WriteLine();

        var courses = await this.courseRepository.GetCoursesAsync().ConfigureAwait(false);
        if (courses.Count == 0)
        {
            Console.WriteLine("No courses available.");
            return;
        }

        var course = this.PromptForCourse(courses);
        var holeSelection = this.PromptForHoleSelection();
        var holes = course.SelectHoles(holeSelection);
        var format = this.PromptForFormat();
        var players = this.CollectPlayers(course, holes, format);

        this.PrintRoundSummary(course, holes, format, players, holeSelection);
        var round = new GameplayRound(course, holes, players, format);
        var runner = new RoundRunner(round, this.shotEngine, this.randomProvider, this.options);
        var trackers = runner.PlayRound();
        this.PrintFinalScores(round, trackers);
    }

    private Course PromptForCourse(IReadOnlyList<Course> courses)
    {
        for (var i = 0; i < courses.Count; i++)
        {
            Console.WriteLine($"[{i + 1}] {courses[i].Name} ({courses[i].Location})");
        }

        Console.Write($"Select course (1-{courses.Count}): ");
        var index = this.ReadInt(value => value >= 1 && value <= courses.Count) - 1;
        return courses[index];
    }

    private HoleSelection PromptForHoleSelection()
    {
        Console.WriteLine("Holes to play: [1] 18, [2] Front 9, [3] Back 9");
        Console.Write("Selection: ");
        return this.ReadInt(value => value is >= 1 and <= 3) switch
        {
            1 => HoleSelection.All18,
            2 => HoleSelection.Front9,
            _ => HoleSelection.Back9,
        };
    }

    private GameFormat PromptForFormat()
    {
        Console.WriteLine("Format: [1] Stroke play, [2] Stableford");
        Console.Write("Selection: ");
        return this.ReadInt(value => value is 1 or 2) == 1 ? GameFormat.StrokePlay : GameFormat.Stableford;
    }

    private List<Player> CollectPlayers(Course course, IReadOnlyList<Hole> holes, GameFormat format)
    {
        Console.WriteLine();
        Console.Write("How many players (1-4)? ");
        var playerCount = this.ReadInt(value => value is >= 1 and <= 4);
        var strokeIndexes = StrokeIndexMapper.MapMenStrokeIndexes(holes);
        var players = new List<Player>(playerCount);

        for (var i = 0; i < playerCount; i++)
        {
            Console.WriteLine();
            Console.WriteLine($"Player {i + 1}");
            Console.Write("Name: ");
            var name = this.ReadRequiredString();
            Console.Write("Handicap (0-54): ");
            var handicap = this.ReadInt(value => value is >= 0 and <= 54);
            var tee = this.PromptForTee(course);
            var eph = EffectiveHandicapCalculator.ComputeEffectivePlayingHandicap(
                handicap,
                format == GameFormat.Stableford,
                holes.Count);
            var allocations = StrokeAllocator.Allocate(eph, strokeIndexes);
            players.Add(new Player(name, handicap, tee, eph, allocations));
        }

        return players;
    }

    private string PromptForTee(Course course)
    {
        Console.WriteLine("Tees:");
        foreach (var tee in course.TeesMetadata.Keys)
        {
            Console.WriteLine($" - {tee}");
        }

        Console.Write($"Tee (Enter for {course.DefaultTee}): ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            return course.DefaultTee;
        }

        var value = input.Trim();
        while (!course.TeesMetadata.ContainsKey(value))
        {
            Console.Write("Unknown tee, try again: ");
            value = this.ReadRequiredString();
        }

        return value;
    }

    private void PrintRoundSummary(
        Course course,
        IReadOnlyList<Hole> holes,
        GameFormat format,
        IReadOnlyList<Player> players,
        HoleSelection selection)
    {
        Console.WriteLine();
        Console.WriteLine("=== Round Summary ===");
        Console.WriteLine($"Course: {course.Name} ({course.Location})");
        Console.WriteLine($"Holes: {this.DescribeSelection(selection)} ({holes.Count})");
        Console.WriteLine($"Format: {format}");
        Console.WriteLine();

        foreach (var player in players)
        {
            Console.WriteLine($"{player.Name} — Tee {player.Tee} | HI {player.HandicapIndex18} → EPH {player.EffectivePlayingHandicap}");
            Console.WriteLine($"Strokes: {string.Join(", ", player.AllocatedStrokesPerHole)}");
            Console.WriteLine();
        }
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance method keeps structure uniform for potential overrides.")]
    private string DescribeSelection(HoleSelection selection)
    {
        return selection switch
        {
            HoleSelection.All18 => "Full 18",
            HoleSelection.Front9 => "Front 9",
            HoleSelection.Back9 => "Back 9",
            _ => selection.ToString(),
        };
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance method keeps access to options if future logic requires it.")]
    private void PrintFinalScores(GameplayRound round, IReadOnlyList<PlayerRoundTracker> trackers)
    {
        Console.WriteLine();
        Console.WriteLine("=== Final Scoreboard ===");

        if (round.Format == GameFormat.Stableford)
        {
            foreach (var tracker in trackers.OrderByDescending(t => t.TotalStablefordPoints))
            {
                Console.WriteLine($"{tracker.Player.Name}: {tracker.TotalStablefordPoints} pts | {string.Join(" ", tracker.StablefordPerHole)}");
            }
        }
        else
        {
            var parTotal = round.Holes.Sum(hole => hole.Par);
            foreach (var tracker in trackers.OrderBy(t => t.TotalNetStrokes))
            {
                var relative = tracker.TotalNetStrokes - parTotal;
                var label = relative switch
                {
                    < 0 => $"{Math.Abs(relative)} under",
                    0 => "even",
                    _ => $"{relative} over",
                };
                Console.WriteLine($"{tracker.Player.Name}: Gross {tracker.TotalGrossStrokes}, Net {tracker.TotalNetStrokes} ({label})");
            }
        }
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance form eases future dependency injection for input.")]
    private int ReadInt(Func<int, bool> predicate)
    {
        while (true)
        {
            var input = Console.ReadLine();
            if (int.TryParse(input, out var value) && predicate(value))
            {
                return value;
            }

            Console.Write("Invalid value, try again: ");
        }
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance form eases future dependency injection for input.")]
    private string ReadRequiredString()
    {
        while (true)
        {
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            Console.Write("Required: ");
        }
    }
}

#pragma warning restore CA1303
