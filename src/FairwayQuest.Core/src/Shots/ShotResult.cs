namespace FairwayQuest.Core.Shots;

/// <summary>
/// Represents the outcome of a simulated golf shot.
/// </summary>
public sealed class ShotResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShotResult"/> class.
    /// </summary>
    /// <param name="strokeIncrement">The stroke increment added by the shot.</param>
    /// <param name="penaltyStrokes">The penalty strokes incurred.</param>
    /// <param name="distance">The distance travelled.</param>
    /// <param name="lateral">The lateral descriptor.</param>
    /// <param name="newLie">The resulting lie.</param>
    /// <param name="newRemaining">The new remaining yardage.</param>
    /// <param name="holed">Whether the ball was holed.</param>
    /// <param name="commentary">The commentary describing the shot.</param>
    public ShotResult(
        int strokeIncrement,
        int penaltyStrokes,
        double distance,
        string lateral,
        Lie newLie,
        double newRemaining,
        bool holed,
        string commentary)
    {
        if (strokeIncrement <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(strokeIncrement), strokeIncrement, "Stroke increment must be positive.");
        }

        if (penaltyStrokes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(penaltyStrokes), penaltyStrokes, "Penalty strokes cannot be negative.");
        }

        this.StrokeIncrement = strokeIncrement;
        this.PenaltyStrokes = penaltyStrokes;
        this.Distance = distance;
        this.Lateral = lateral;
        this.NewLie = newLie;
        this.NewRemaining = Math.Max(0, newRemaining);
        this.Holed = holed;
        this.Commentary = commentary;
    }

    /// <summary>Gets the number of strokes added by the shot.</summary>
    public int StrokeIncrement { get; }

    /// <summary>Gets the penalty strokes incurred by the shot.</summary>
    public int PenaltyStrokes { get; }

    /// <summary>Gets the distance travelled in yards.</summary>
    public double Distance { get; }

    /// <summary>Gets the lateral description (e.g., fade, draw).</summary>
    public string Lateral { get; }

    /// <summary>Gets the new lie after the shot.</summary>
    public Lie NewLie { get; }

    /// <summary>Gets the remaining distance in yards.</summary>
    public double NewRemaining { get; }

    /// <summary>Gets a value indicating whether the shot holed out.</summary>
    public bool Holed { get; }

    /// <summary>Gets the commentary describing the outcome.</summary>
    public string Commentary { get; }
}
