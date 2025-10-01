namespace FairwayQuest.Core.Gameplay;

using FairwayQuest.Core.Shots;

/// <summary>
/// Tracks per-hole progress for a player.
/// </summary>
public sealed class PlayerHoleState
{
    private readonly List<string> commentaryLog = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerHoleState"/> class.
    /// </summary>
    /// <param name="startingYards">The starting yardage from the tee.</param>
    public PlayerHoleState(double startingYards)
    {
        if (startingYards <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startingYards), startingYards, "Starting yardage must be positive.");
        }

        this.RemainingYards = startingYards;
        this.Lie = Lie.Tee;
    }

    /// <summary>Gets the remaining yardage to the pin.</summary>
    public double RemainingYards { get; private set; }

    /// <summary>Gets the current lie.</summary>
    public Lie Lie { get; private set; }

    /// <summary>Gets the number of strokes taken on the hole.</summary>
    public int StrokeCount { get; private set; }

    /// <summary>Gets the accumulated penalty strokes.</summary>
    public int PenaltyStrokes { get; private set; }

    /// <summary>Gets a value indicating whether the player has holed out.</summary>
    public bool IsHoled { get; private set; }

    /// <summary>Gets the commentary lines produced for each shot.</summary>
    public IReadOnlyList<string> Commentary => this.commentaryLog;

    /// <summary>Gets the gross strokes including penalties.</summary>
    public int GrossStrokes => this.StrokeCount + this.PenaltyStrokes;

    /// <summary>
    /// Applies a simulated shot result to the current state.
    /// </summary>
    /// <param name="result">The shot result to apply.</param>
    public void ApplyShot(ShotResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        this.StrokeCount += result.StrokeIncrement;
        this.PenaltyStrokes += result.PenaltyStrokes;
        this.RemainingYards = result.NewRemaining;
        this.Lie = result.NewLie;
        this.IsHoled = result.Holed;
        this.commentaryLog.Add(result.Commentary);
    }
}
