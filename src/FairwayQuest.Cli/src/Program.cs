namespace FairwayQuest.Cli;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FairwayQuest.Courses;
using FairwayQuest.Core.Abstractions;
using FairwayQuest.Core.Allocation;
using FairwayQuest.Core.Handicapping;
using FairwayQuest.Core.Models;
using FairwayQuest.Core.Scoring;

internal static class Program
{
    private static async Task<int> Main()
    {
        try
        {
            var repository = new JsonCourseRepository(ResolveCoursesPath());
            var app = new Application(repository);
            await app.RunAsync().ConfigureAwait(false);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static string ResolveCoursesPath()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.CurrentDirectory, "courses"),
            Path.Combine(AppContext.BaseDirectory, "courses"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "courses")),
        };

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return candidates[0];
    }

    private sealed class Application
    {
        private readonly ICourseRepository courseRepository;

        public Application(ICourseRepository courseRepository)
        {
            this.courseRepository = courseRepository;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("FairwayQuest - Handicap & Stableford Demo");
            Console.WriteLine();

            var courses = await courseRepository.GetCoursesAsync().ConfigureAwait(false);
            if (courses.Count == 0)
            {
                Console.WriteLine("No courses available.");
                return;
            }

            var course = PromptForCourse(courses);
            var tee = PromptForTee(course);
            var selection = PromptForHoleSelection();
            var holes = course.SelectHoles(selection);
            var format = PromptForFormat();

            var players = CollectPlayers(format, holes);
            DisplayRoundSummary(course, tee, selection, holes, players);
            PlayRound(players, holes, format);
            PrintFinalScores(players, format);
        }

        private static IReadOnlyList<PlayerRound> CollectPlayers(GameFormat format, IReadOnlyList<Hole> holes)
        {
            Console.WriteLine();
            Console.Write("How many players (1-4)? ");
            var playerCount = PromptForInt(value => value is >= 1 and <= 4);

            var players = new List<PlayerRound>(playerCount);
            for (var i = 0; i < playerCount; i++)
            {
                Console.Write($"Player {i + 1} name: ");
                var name = PromptForNonEmpty();

                Console.Write($"{name}'s handicap index (0-54): ");
                var handicap = PromptForInt(value => value is >= 0 and <= 54);

                var effective = EffectiveHandicapCalculator.ComputeEffectivePlayingHandicap(
                    handicap,
                    format == GameFormat.Stableford,
                    holes.Count);

                var strokeIndexes = StrokeIndexMapper.MapMenStrokeIndexes(holes);
                var allocations = StrokeAllocator.Allocate(effective, strokeIndexes);
                players.Add(new PlayerRound(name, handicap, effective, allocations));
            }

            return players;
        }

        private static void DisplayRoundSummary(
            Course course,
            string tee,
            HoleSelection selection,
            IReadOnlyList<Hole> holes,
            IReadOnlyList<PlayerRound> players)
        {
            Console.WriteLine();
            Console.WriteLine($"Course: {course.Name} ({course.Location}) - Tee: {tee}");
            Console.WriteLine($"Playing {DescribeSelection(selection)} ({holes.Count} holes)");
            Console.WriteLine();

            foreach (var player in players)
            {
                Console.WriteLine($"{player.Name}: HI={player.HandicapIndex} → EPH={player.EffectiveHandicap}");
                Console.WriteLine($"Strokes per hole: {string.Join(", ", player.StrokesPerHole)}");
                Console.WriteLine();
            }
        }

        private static void PlayRound(IReadOnlyList<PlayerRound> players, IReadOnlyList<Hole> holes, GameFormat format)
        {
            for (var holeIndex = 0; holeIndex < holes.Count; holeIndex++)
            {
                var hole = holes[holeIndex];
                Console.WriteLine($"Hole {hole.Number} (Par {hole.Par}, SI {hole.StrokeIndex.Men})");

                foreach (var player in players)
                {
                    Console.Write($"  {player.Name} gross strokes: ");
                    var gross = PromptForInt(value => value is > 0 and <= 20);
                    var strokesReceived = player.StrokesPerHole[holeIndex];

                    if (format == GameFormat.Stableford)
                    {
                        var result = ScoreCalculator.CalculateStableford(hole.Par, gross, strokesReceived);
                        player.AddStablefordResult(result);
                        Console.WriteLine($"    → Net {result.NetStrokes} ({DescribeRelative(result.NetStrokes - hole.Par)}) → {result.StablefordPoints} pts");
                    }
                    else
                    {
                        player.AddStrokePlayResult(gross, strokesReceived);
                        Console.WriteLine($"    → Gross {gross} with {strokesReceived} stroke(s)");
                    }
                }

                Console.WriteLine();
            }
        }

        private static void PrintFinalScores(IReadOnlyList<PlayerRound> players, GameFormat format)
        {
            Console.WriteLine("=== Final Scores ===");
            if (format == GameFormat.Stableford)
            {
                foreach (var player in players.OrderByDescending(p => p.TotalStablefordPoints))
                {
                    Console.WriteLine($"{player.Name}: {player.TotalStablefordPoints} points");
                }
            }
            else
            {
                foreach (var player in players.OrderBy(p => p.TotalGrossStrokes))
                {
                    Console.WriteLine($"{player.Name}: Gross {player.TotalGrossStrokes}, Net {player.NetStrokePlayScore}");
                }
            }
        }

        private static string DescribeSelection(HoleSelection selection)
        {
            return selection switch
            {
                HoleSelection.All18 => "18 holes",
                HoleSelection.Front9 => "Front 9",
                HoleSelection.Back9 => "Back 9",
                _ => selection.ToString(),
            };
        }

        private static string DescribeRelative(int relative)
        {
            return relative switch
            {
                < 0 => $"{Math.Abs(relative)} under",
                0 => "even",
                _ => $"{relative} over",
            };
        }

        private static Course PromptForCourse(IReadOnlyList<Course> courses)
        {
            for (var i = 0; i < courses.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {courses[i].Name} ({courses[i].Location})");
            }

            Console.Write("Select course: ");
            var index = PromptForInt(value => value is >= 1 and <= courses.Count) - 1;
            return courses[index];
        }

        private static string PromptForTee(Course course)
        {
            Console.WriteLine("Available tees:");
            foreach (var tee in course.TeesMetadata.Keys)
            {
                Console.WriteLine($" - {tee}");
            }

            Console.Write($"Choose tee (Enter for {course.DefaultTee}): ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return course.DefaultTee;
            }

            var trimmed = input.Trim();
            while (!course.TeesMetadata.ContainsKey(trimmed))
            {
                Console.Write("Unknown tee. Try again: ");
                trimmed = PromptForNonEmpty();
            }

            return trimmed;
        }

        private static HoleSelection PromptForHoleSelection()
        {
            Console.WriteLine("Holes to play: [1] 18, [2] Front 9, [3] Back 9");
            Console.Write("Selection: ");
            return PromptForInt(value => value is >= 1 and <= 3) switch
            {
                1 => HoleSelection.All18,
                2 => HoleSelection.Front9,
                _ => HoleSelection.Back9,
            };
        }

        private static GameFormat PromptForFormat()
        {
            Console.WriteLine("Format: [1] Stroke play, [2] Stableford");
            Console.Write("Selection: ");
            return PromptForInt(value => value is 1 or 2) == 1 ? GameFormat.StrokePlay : GameFormat.Stableford;
        }

        private static int PromptForInt(Func<int, bool> validator)
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (int.TryParse(input, out var value) && validator(value))
                {
                    return value;
                }

                Console.Write("Invalid value, try again: ");
            }
        }

        private static string PromptForNonEmpty()
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input.Trim();
                }

                Console.Write("Value required: ");
            }
        }

        private sealed class PlayerRound
        {
            private readonly List<HoleScore> stablefordResults = new();

            public PlayerRound(string name, int handicapIndex, int effectiveHandicap, IReadOnlyList<int> strokesPerHole)
            {
                Name = name;
                HandicapIndex = handicapIndex;
                EffectiveHandicap = effectiveHandicap;
                StrokesPerHole = strokesPerHole;
            }

            public string Name { get; }

            public int HandicapIndex { get; }

            public int EffectiveHandicap { get; }

            public IReadOnlyList<int> StrokesPerHole { get; }

            public int TotalStablefordPoints => stablefordResults.Sum(result => result.StablefordPoints);

            public int TotalGrossStrokes { get; private set; }

            public int NetStrokePlayScore { get; private set; }

            public void AddStablefordResult(HoleScore score)
            {
                stablefordResults.Add(score);
            }

            public void AddStrokePlayResult(int gross, int strokesReceived)
            {
                TotalGrossStrokes += gross;
                NetStrokePlayScore += gross - strokesReceived;
            }
        }
    }
}
