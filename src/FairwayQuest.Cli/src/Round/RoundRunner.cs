namespace FairwayQuest.Cli.Round;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FairwayQuest.Core.Abstractions;
using FairwayQuest.Core.Gameplay;
using FairwayQuest.Core.Models;
using FairwayQuest.Core.Scoring;
using FairwayQuest.Core.Shots;
using GameplayRound = FairwayQuest.Core.Gameplay.Round;

#pragma warning disable CA1303 // CLI intentionally writes literal strings to the console.

/// <summary>
/// Coordinates the shot-by-shot simulation for a configured round.
/// </summary>
internal sealed class RoundRunner
{
    private readonly GameplayRound round;
    private readonly ShotEngine shotEngine;
    private readonly IRandomProvider randomProvider;
    private readonly AppOptions options;
    private readonly ClubAdvisor advisor;
    private readonly IShotSelector shotSelector;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundRunner"/> class.
    /// </summary>
    /// <param name="round">The configured round.</param>
    /// <param name="shotEngine">The shot simulation engine.</param>
    /// <param name="randomProvider">Deterministic random provider.</param>
    /// <param name="options">Application options controlling narration.</param>
    /// <param name="advisor">Optional advisor override.</param>
    /// <param name="shotSelector">Optional shot selector override.</param>
    public RoundRunner(
        GameplayRound round,
        ShotEngine shotEngine,
        IRandomProvider randomProvider,
        AppOptions options,
        ClubAdvisor? advisor = null,
        IShotSelector? shotSelector = null)
    {
        this.round = round;
        this.shotEngine = shotEngine;
        this.randomProvider = randomProvider;
        this.options = options;
        this.advisor = advisor ?? new ClubAdvisor();
        this.shotSelector = shotSelector ?? new ShotPrompter(options, this.advisor);
    }

    /// <summary>
    /// Plays the configured round to completion.
    /// </summary>
    /// <returns>Round trackers containing scoring aggregates.</returns>
    public IReadOnlyList<PlayerRoundTracker> PlayRound()
    {
        var trackers = this.round.Players.Select(player => new PlayerRoundTracker(player)).ToList();
        for (var holeIndex = 0; holeIndex < this.round.Holes.Count; holeIndex++)
        {
            this.PlayHole(holeIndex, trackers);
        }

        return trackers;
    }

    private void PlayHole(int holeIndex, List<PlayerRoundTracker> trackers)
    {
        var hole = this.round.Holes[holeIndex];
        this.PrintHoleHeader(hole);
        var holeState = this.CreateHoleState(hole);
        this.RunHole(holeState);
        this.SummarizeHole(holeIndex, holeState, trackers);
    }

    private HolePlayState CreateHoleState(Hole hole)
    {
        var states = this.round.Players.Select(player => new PlayerHoleState(this.ResolveYardage(hole, player))).ToList();
        return new HolePlayState(hole, this.round.Players, states);
    }

    private void PrintHoleHeader(Hole hole)
    {
        Console.WriteLine();
        Console.WriteLine($"--- Hole {hole.Number} | Par {hole.Par} ---");
    }

    private double ResolveYardage(Hole hole, Player player)
    {
        if (!hole.Yards.TryGetValue(player.Tee, out var yardage))
        {
            throw new InvalidOperationException($"Tee '{player.Tee}' missing for hole {hole.Number}.");
        }

        return yardage;
    }

    private void RunHole(HolePlayState holeState)
    {
        while (!holeState.IsComplete)
        {
            this.ExecuteRotation(holeState);
        }
    }

    private void ExecuteRotation(HolePlayState holeState)
    {
        for (var playerIndex = 0; playerIndex < this.round.Players.Count; playerIndex++)
        {
            this.ProcessTurn(holeState, playerIndex);
        }
    }

    private void ProcessTurn(HolePlayState holeState, int playerIndex)
    {
        var state = holeState.PlayerStates[playerIndex];
        if (state.IsHoled)
        {
            return;
        }

        this.TakeShot(holeState, playerIndex);
    }

    private void TakeShot(HolePlayState holeState, int playerIndex)
    {
        var player = this.round.Players[playerIndex];
        var state = holeState.PlayerStates[playerIndex];
        var club = this.shotSelector.SelectClub(player, holeState.Hole, state);
        var request = new ShotRequest(state.Lie, state.RemainingYards, club);
        var result = this.shotEngine.Execute(request, this.randomProvider);
        state.ApplyShot(result);
        Console.WriteLine(result.Commentary);
        if (!this.options.FastMode)
        {
            Console.WriteLine();
        }
    }

    private void SummarizeHole(int holeIndex, HolePlayState holeState, List<PlayerRoundTracker> trackers)
    {
        Console.WriteLine($"Hole {holeState.Hole.Number} summary:");
        for (var playerIndex = 0; playerIndex < this.round.Players.Count; playerIndex++)
        {
            this.SummarizePlayerHole(holeIndex, holeState, trackers, playerIndex);
        }
    }

    private void SummarizePlayerHole(int holeIndex, HolePlayState holeState, List<PlayerRoundTracker> trackers, int playerIndex)
    {
        var player = this.round.Players[playerIndex];
        var state = holeState.PlayerStates[playerIndex];
        var strokesReceived = player.AllocatedStrokesPerHole[holeIndex];
        var gross = state.GrossStrokes;

        if (this.round.Format == GameFormat.Stableford)
        {
            this.SummarizeStableford(holeState, trackers, playerIndex, player, strokesReceived, gross);
            return;
        }

        this.SummarizeStrokePlay(trackers, playerIndex, player, strokesReceived, gross, holeState.Hole.Par);
    }

    private void SummarizeStableford(
        HolePlayState holeState,
        List<PlayerRoundTracker> trackers,
        int playerIndex,
        Player player,
        int strokesReceived,
        int gross)
    {
        var score = ScoreCalculator.CalculateStableford(holeState.Hole.Par, gross, strokesReceived);
        trackers[playerIndex].RecordStableford(score.GrossStrokes, score.NetStrokes, score.StablefordPoints);
        Console.WriteLine(
            $" {player.Name}: Gross {gross}, Net {score.NetStrokes} ({this.DescribeRelative(score.NetStrokes - holeState.Hole.Par)}) → {score.StablefordPoints} pts");
    }

    private void SummarizeStrokePlay(
        List<PlayerRoundTracker> trackers,
        int playerIndex,
        Player player,
        int strokesReceived,
        int gross,
        int par)
    {
        var net = gross - strokesReceived;
        trackers[playerIndex].RecordStrokePlay(gross, net);
        Console.WriteLine($" {player.Name}: Gross {gross}, Net {net} ({this.DescribeRelative(net - par)})");
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance method keeps symmetry with other helpers.")]
    private string DescribeRelative(int relative)
    {
        return relative switch
        {
            < 0 => $"{Math.Abs(relative)} under",
            0 => "even",
            _ => $"{relative} over",
        };
    }
}

#pragma warning restore CA1303
