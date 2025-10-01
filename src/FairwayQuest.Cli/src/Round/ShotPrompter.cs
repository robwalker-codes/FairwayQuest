namespace FairwayQuest.Cli.Round;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FairwayQuest.Core.Gameplay;
using FairwayQuest.Core.Models;
using FairwayQuest.Core.Shots;

#pragma warning disable CA1303 // CLI intentionally writes literal strings to the console.

/// <summary>
/// Interactive implementation of <see cref="IShotSelector"/> prompting the user for club choices.
/// </summary>
internal sealed class ShotPrompter : IShotSelector
{
    private readonly AppOptions options;
    private readonly ClubAdvisor advisor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShotPrompter"/> class.
    /// </summary>
    /// <param name="options">Application options controlling automation.</param>
    /// <param name="advisor">Advisor used for auto-suggesting clubs.</param>
    public ShotPrompter(AppOptions options, ClubAdvisor advisor)
    {
        this.options = options;
        this.advisor = advisor;
    }

    /// <inheritdoc />
    public string SelectClub(Player player, Hole hole, PlayerHoleState state)
    {
        var suggestion = this.advisor.Suggest(state.Lie, state.RemainingYards, hole.Par);
        return this.options.AutoPlay
            ? this.HandleAutoPlay(player, hole, state, suggestion)
            : this.PromptForClub(player, hole, state, suggestion);
    }

    private string HandleAutoPlay(Player player, Hole hole, PlayerHoleState state, string suggestion)
    {
        Console.WriteLine(this.FormatHeader(player, hole, state));
        Console.WriteLine($"Auto selects {this.advisor.GetDefinition(suggestion).Label}");
        return suggestion;
    }

    private string PromptForClub(Player player, Hole hole, PlayerHoleState state, string suggestion)
    {
        while (true)
        {
            this.DisplayPrompt(player, hole, state, suggestion);
            var input = Console.ReadLine();
            var selection = this.ProcessInput(input, state, suggestion);
            if (!string.IsNullOrEmpty(selection))
            {
                return selection;
            }
        }
    }

    private void DisplayPrompt(Player player, Hole hole, PlayerHoleState state, string suggestion)
    {
        Console.WriteLine(this.FormatHeader(player, hole, state));
        Console.Write($"Select club (? for list, 'auto' for {suggestion}, 'list' for shots, 'quit' to exit): ");
    }

    private string? ProcessInput(string? input, PlayerHoleState state, string suggestion)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var trimmed = input.Trim();
        if (this.TryResolveCommand(trimmed, state, suggestion, out var commandResult))
        {
            return commandResult;
        }

        if (this.TryNormalizeClub(trimmed, state, out var normalizedClub))
        {
            return normalizedClub;
        }

        Console.WriteLine("Unknown club. Enter '?' for options.");
        return null;
    }

    private bool TryResolveCommand(string input, PlayerHoleState state, string suggestion, out string? resolved)
    {
        resolved = null;
        if (string.Equals(input, "?", StringComparison.OrdinalIgnoreCase))
        {
            this.PrintHelp();
            return true;
        }

        if (string.Equals(input, "auto", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Using suggested {this.advisor.GetDefinition(suggestion).Label}");
            resolved = suggestion;
            return true;
        }

        if (string.Equals(input, "list", StringComparison.OrdinalIgnoreCase))
        {
            this.PrintShotHistory(state);
            return true;
        }

        if (string.Equals(input, "quit", StringComparison.OrdinalIgnoreCase))
        {
            throw new OperationCanceledException();
        }

        return false;
    }

    /// <summary>
    /// Displays the available clubs and commands.
    /// </summary>
    private void PrintHelp()
    {
        Console.WriteLine("Available clubs:");
        foreach (var club in this.advisor.AllClubs())
        {
            Console.WriteLine(club.IsPutter ? $" - {club.Label}: putter" : $" - {club.Label}: {club.Min:0}-{club.Max:0}y");
        }

        Console.WriteLine("Commands: ?, auto, list, quit");
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Maintained as instance for potential option-aware output.")]
    private void PrintShotHistory(PlayerHoleState state)
    {
        if (state.Commentary.Count == 0)
        {
            Console.WriteLine("No shots yet.");
            return;
        }

        Console.WriteLine("Shot history:");
        for (var i = 0; i < state.Commentary.Count; i++)
        {
            Console.WriteLine($" {i + 1}. {state.Commentary[i]}");
        }
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Uses instance context for future customization.")]
    private string FormatHeader(Player player, Hole hole, PlayerHoleState state)
    {
        return $"[{player.Name}] H{hole.Number} | Par {hole.Par} | Yardage {hole.Yards[player.Tee]}y ({player.Tee} tee) | Rem {state.RemainingYards:0}y | Lie {state.Lie}";
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Retained as instance to share advisor context if required.")]
    private bool TryNormalizeClub(string clubCode, PlayerHoleState state, out string normalized)
    {
        normalized = string.Empty;

        if (state.Lie == Lie.Green && !string.Equals(clubCode, "p", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("On the green you must use the putter.");
            return false;
        }

        var match = ShotEngine
            .GetSupportedClubs()
            .FirstOrDefault(club => string.Equals(club.Code, clubCode, StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            return false;
        }

        normalized = match.Code;
        return true;
    }
}

#pragma warning restore CA1303
