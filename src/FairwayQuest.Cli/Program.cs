using System;
using System.Collections.Generic;
using System.Linq;
using FairwayQuest.Core.Courses;
using FairwayQuest.Core.Models;
using FairwayQuest.Core.Scoring;
using FairwayQuest.Core.Simulation;
using FairwayQuest.Core.Validation;

var options = ParseOptions(args);
var rng = new RandomNumberGeneratorAdapter(new Random(options.Seed));
var shotEngine = new ShotEngine();

Console.WriteLine("Welcome to FairwayQuest!");
Console.WriteLine(options.FastMode
    ? "Fast mode is enabled. Essential information will be displayed."
    : "Enjoy a narrated round with full commentary.");

var playerCount = PromptForValue<int>("Enter number of players (1-4): ", InputParsers.TryParsePlayerCount);
var playerEntries = new List<(string Name, int HandicapIndex18)>();
for (var i = 0; i < playerCount; i++)
{
    var name = PromptForNonEmpty($"Player {i + 1} name: ");
    var handicap = PromptForValue<int>($"{name}'s handicap (0-54): ", InputParsers.TryParseHandicap);
    playerEntries.Add((name, handicap));
}

var holeCount = PromptForValue<int>("Number of holes (9 or 18): ", InputParsers.TryParseHoleCount);
var gameType = PromptForValue<GameType>("Game type (stroke/stableford): ", InputParsers.TryParseGameType);

var course = DefaultCourseProvider.CreateCourse(holeCount);
var strokeIndexes = course.Select(h => h.StrokeIndex).ToArray();
var players = new List<Player>();
var preAllowanceHandicaps = new List<(Player player, int baseHandicap)>();

foreach (var entry in playerEntries)
{
    var playingHandicap = holeCount == 18
        ? entry.HandicapIndex18
        : (int)Math.Round(entry.HandicapIndex18 / 2.0, MidpointRounding.ToEven);

    var effective = Handicap.ComputeEffectiveHandicap(
        entry.HandicapIndex18,
        holeCount,
        gameType == GameType.Stableford,
        options.StablefordAllowancePercent);

    var allocation = Handicap.AllocateStrokesPerHole(effective, strokeIndexes);

    var player = new Player
    {
        Name = entry.Name,
        HandicapIndex18 = entry.HandicapIndex18,
        EffectiveHandicap = effective,
        AllocatedStrokesPerHole = allocation
    };

    players.Add(player);

    preAllowanceHandicaps.Add((player, playingHandicap));
}

if (gameType == GameType.Stableford)
{
    Console.WriteLine();
    Console.WriteLine("Stableford handicap allocations:");
    foreach (var (player, baseHandicap) in preAllowanceHandicaps)
    {
        var baseLabel = holeCount == 18 ? "18-hole eff" : "9-hole eff";
        Console.WriteLine($"{player.Name}: HI18={player.HandicapIndex18} → {baseLabel}={baseHandicap} → Stableford {options.StablefordAllowancePercent}% → {player.EffectiveHandicap}");
        Console.WriteLine($"Per-hole strokes (by hole #): [{string.Join(",", player.AllocatedStrokesPerHole)}]");
    }
}

var playerStates = players.Select(player => new PlayerRoundState(player)).ToList();

for (var holeIndex = 0; holeIndex < course.Count; holeIndex++)
{
    var hole = course[holeIndex];
    Console.WriteLine();
    Console.WriteLine($"=== Hole {hole.Number} | Par {hole.Par} | {hole.Yardage}y ===");
    DisplayStandings(playerStates, gameType, options.FastMode);

    foreach (var state in playerStates)
    {
        state.BeginHole(hole);
        Console.WriteLine($"{state.Player.Name} starts {hole.Yardage}y away on a par {hole.Par}.");

        while (!state.HoleComplete)
        {
            var banner = $"[{state.Player.Name}] Hole {hole.Number} | Par {hole.Par} | {hole.Yardage}y | Remaining {Math.Round(state.CurrentShotState.RemainingYards)}y | Lie {state.CurrentShotState.Lie}";
            Console.WriteLine(banner);
            var club = PromptForClub(state.CurrentShotState.IsOnGreen);
            var outcome = shotEngine.ExecuteShot(state.CurrentShotState, club, rng);
            state.ApplyShot(outcome);
            Console.WriteLine(outcome.Description);
        }

        state.CompleteHole(hole, gameType);
    }
}

Console.WriteLine();
Console.WriteLine("=== Final Scoreboard ===");
PrintFinalScoreboard(playerStates, course, gameType);

return;

static string PromptForClub(bool onGreen)
{
    while (true)
    {
        Console.Write("Select club (? for list): ");
        var input = Console.ReadLine() ?? string.Empty;
        if (input.Trim() == "?")
        {
            Console.WriteLine("Available clubs: " + string.Join(", ", ClubCatalog.NonPutterCodes) + ", p (green only)");
            continue;
        }

        if (InputParsers.TryParseClub(input, onGreen, out var club))
        {
            return club;
        }

        Console.WriteLine(onGreen ? "Use 'p' when putting." : "Enter a valid club code (e.g., D, 7i, pw).");
    }
}

static T PromptForValue<T>(string prompt, TryParseDelegate<T> parser)
{
    while (true)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();
        if (parser(input, out var value))
        {
            return value;
        }

        Console.WriteLine("Invalid input. Please try again.");
    }
}

static string PromptForNonEmpty(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(input))
        {
            return input.Trim();
        }

        Console.WriteLine("Value cannot be empty.");
    }
}

static void DisplayStandings(IEnumerable<PlayerRoundState> players, GameType gameType, bool fastMode)
{
    Console.WriteLine(fastMode ? "Standings:" : "Current standings before this hole:");
    foreach (var state in players)
    {
        if (gameType == GameType.Stroke)
        {
            var gross = state.TotalGross;
            var parSoFar = state.TotalParPlayed;
            var relative = parSoFar == 0 ? 0 : gross - parSoFar;
            var net = state.TotalNet;
            var netRelative = parSoFar == 0 ? 0 : net - parSoFar;
            Console.WriteLine($" - {state.Player.Name}: {gross} strokes ({FormatRelative(relative)}) | Net {net} ({FormatRelative(netRelative)})");
        }
        else
        {
            Console.WriteLine($" - {state.Player.Name}: {state.TotalStablefordPoints} pts");
        }
    }
}

static void PrintFinalScoreboard(IEnumerable<PlayerRoundState> players, IReadOnlyList<Hole> course, GameType gameType)
{
    if (gameType == GameType.Stroke)
    {
        var totalPar = course.Sum(h => h.Par);
        Console.WriteLine($"{"Player",-15} {"Gross",5} {"Net",5} {"±Gross",8} {"±Net",6}");
        foreach (var state in players)
        {
            var gross = state.TotalGross;
            var net = state.TotalNet;
            var grossRelative = gross - totalPar;
            var netRelative = net - totalPar;
            Console.WriteLine($"{state.Player.Name,-15} {gross,5} {net,5} {FormatRelative(grossRelative),8} {FormatRelative(netRelative),6}");
        }
    }
    else
    {
        Console.WriteLine($"{"Player",-15} {"Pts",5} | Per-hole points");
        foreach (var state in players)
        {
            var perHole = string.Join(" ", state.StablefordPoints.Select(p => p.ToString().PadLeft(2)));
            Console.WriteLine($"{state.Player.Name,-15} {state.TotalStablefordPoints,5} | {perHole}");
        }
    }
}

static string FormatRelative(int relative)
{
    return relative switch
    {
        < 0 => relative.ToString(),
        > 0 => $"+{relative}",
        _ => "E"
    };
}

static CliOptions ParseOptions(string[] args)
{
    var options = new CliOptions();
    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];
        switch (arg)
        {
            case "--seed":
                if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out var seed))
                {
                    throw new ArgumentException("--seed requires an integer value");
                }

                options.Seed = seed;
                i++;
                break;
            case "--fast":
                options.FastMode = true;
                break;
            case "--stableford-allowance":
                if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out var allowance))
                {
                    throw new ArgumentException("--stableford-allowance requires an integer value");
                }

                if (allowance < 0)
                {
                    throw new ArgumentException("--stableford-allowance must be non-negative");
                }

                options.StablefordAllowancePercent = allowance;
                i++;
                break;
            default:
                throw new ArgumentException($"Unknown argument '{arg}'");
        }
    }

    return options;
}

delegate bool TryParseDelegate<T>(string? input, out T value);

sealed class CliOptions
{
    public int Seed { get; set; } = 90210;
    public bool FastMode { get; set; }
    public int StablefordAllowancePercent { get; set; } = 95;
}

sealed class PlayerRoundState
{
    private Hole? _currentHole;
    private int _strokesThisHole;
    private ShotState _currentState = ShotState.ForHoleStart(1);

    public PlayerRoundState(Player player)
    {
        Player = player;
    }

    public Player Player { get; }
    public List<int> GrossStrokes { get; } = new();
    public List<int> NetStrokes { get; } = new();
    public List<int> StablefordPoints { get; } = new();
    public List<int> ParsPlayed { get; } = new();

    public ShotState CurrentShotState => _currentState;
    public bool HoleComplete => _currentHole is not null && _currentState.RemainingYards <= 0.5;
    public int HolesCompleted => GrossStrokes.Count;
    public int TotalGross => GrossStrokes.Sum();
    public int TotalNet => NetStrokes.Sum();
    public int TotalStablefordPoints => StablefordPoints.Sum();
    public int TotalParPlayed => ParsPlayed.Sum();

    public void BeginHole(Hole hole)
    {
        _currentHole = hole;
        _currentState = ShotState.ForHoleStart(hole.Yardage);
        _strokesThisHole = 0;
    }

    public void ApplyShot(ShotOutcome outcome)
    {
        _strokesThisHole += outcome.StrokesUsed;
        _currentState = outcome.NewState;
    }

    public void CompleteHole(Hole hole, GameType gameType)
    {
        if (_currentHole is null)
        {
            return;
        }

        GrossStrokes.Add(_strokesThisHole);
        ParsPlayed.Add(hole.Par);
        var allocation = Player.AllocatedStrokesPerHole[hole.Number - 1];
        var net = ScoreCalculator.CalculateNetStrokes(_strokesThisHole, allocation);
        NetStrokes.Add(net);

        if (gameType == GameType.Stableford)
        {
            var relative = net - hole.Par;
            var points = ScoreCalculator.StablefordPointsFromRelativeScore(relative);
            StablefordPoints.Add(points);
            Console.WriteLine($"{Player.Name} earns {points} Stableford point(s).");
        }
        else
        {
            Console.WriteLine($"{Player.Name} records {_strokesThisHole} stroke(s). Net: {net}.");
        }

        _currentHole = null;
        _strokesThisHole = 0;
        _currentState = ShotState.ForHoleStart(1);
    }
}
