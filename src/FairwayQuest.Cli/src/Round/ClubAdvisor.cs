namespace FairwayQuest.Cli.Round;

using System;
using System.Collections.Generic;
using System.Linq;
using FairwayQuest.Core.Shots;

/// <summary>
/// Provides heuristics for recommending clubs based on remaining yardage and lie.
/// </summary>
internal sealed class ClubAdvisor
{
    private readonly List<ClubDefinition> clubs;
    private readonly List<ClubDefinition> fullSwingClubs;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClubAdvisor"/> class.
    /// </summary>
    public ClubAdvisor()
    {
        this.clubs = ShotEngine.GetSupportedClubs().ToList();
        this.fullSwingClubs = this.clubs
            .Where(club => !club.IsPutter)
            .OrderBy(club => club.Average)
            .ToList();
    }

    /// <summary>
    /// Suggests a club code based on the current context.
    /// </summary>
    /// <param name="lie">The current lie.</param>
    /// <param name="remainingYards">The remaining distance to the pin.</param>
    /// <param name="holePar">The par of the hole.</param>
    /// <returns>The recommended club code.</returns>
    public string Suggest(Lie lie, double remainingYards, int holePar)
    {
        if (lie == Lie.Green)
        {
            return "p";
        }

        var shortGame = ResolveShortGameSuggestion(lie, remainingYards);
        if (shortGame is not null)
        {
            return shortGame;
        }

        if (ShouldUseDriver(lie, holePar, remainingYards))
        {
            return "d";
        }

        return this.SuggestFullSwingClub(lie, remainingYards);
    }

    /// <summary>
    /// Retrieves the definition for a particular club code.
    /// </summary>
    /// <param name="code">The club code to lookup.</param>
    /// <returns>The matching club definition.</returns>
    public ClubDefinition GetDefinition(string code)
    {
        var match = this.clubs.FirstOrDefault(club => string.Equals(club.Code, code, StringComparison.OrdinalIgnoreCase));
        return match ?? throw new InvalidOperationException($"Unknown club code '{code}'.");
    }

    /// <summary>
    /// Enumerates all known clubs.
    /// </summary>
    /// <returns>The ordered collection of club definitions.</returns>
    public IEnumerable<ClubDefinition> AllClubs() => this.clubs;

    private static string? ResolveShortGameSuggestion(Lie lie, double remainingYards)
    {
        if (remainingYards <= 12)
        {
            return lie == Lie.Fringe ? "p" : "lw";
        }

        if (remainingYards <= 25)
        {
            return "sw";
        }

        if (remainingYards <= 40)
        {
            return "pw";
        }

        if (lie == Lie.Bunker)
        {
            return "sw";
        }

        return null;
    }

    private static bool ShouldUseDriver(Lie lie, int holePar, double remainingYards)
    {
        return lie == Lie.Tee && holePar >= 4 && remainingYards >= 220;
    }

    private string SuggestFullSwingClub(Lie lie, double remainingYards)
    {
        var adjustedTarget = remainingYards / GetLieMultiplier(lie);
        var match = this.fullSwingClubs.FirstOrDefault(club => adjustedTarget <= club.Average + 8);
        return match?.Code ?? this.fullSwingClubs[^1].Code;
    }

    private static double GetLieMultiplier(Lie lie)
    {
        return lie switch
        {
            Lie.Rough => 0.9,
            Lie.DeepRough => 0.8,
            Lie.Bunker => 0.85,
            Lie.Fringe => 0.95,
            _ => 1,
        };
    }
}
