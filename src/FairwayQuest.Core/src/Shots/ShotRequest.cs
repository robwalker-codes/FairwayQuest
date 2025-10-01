namespace FairwayQuest.Core.Shots;

/// <summary>
/// Defines the information required to simulate a golf shot.
/// </summary>
public sealed class ShotRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShotRequest"/> class.
    /// </summary>
    /// <param name="lie">The current lie.</param>
    /// <param name="remainingYards">The remaining distance to the pin in yards.</param>
    /// <param name="club">The club code being used.</param>
    public ShotRequest(Lie lie, double remainingYards, string club)
    {
        if (remainingYards < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(remainingYards), remainingYards, "Remaining distance cannot be negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(club);

        this.Lie = lie;
        this.RemainingYards = remainingYards;
        this.Club = club.Trim();
    }

    /// <summary>
    /// Gets the current lie.
    /// </summary>
    public Lie Lie { get; }

    /// <summary>
    /// Gets the remaining distance in yards.
    /// </summary>
    public double RemainingYards { get; }

    /// <summary>
    /// Gets the selected club code.
    /// </summary>
    public string Club { get; }
}
