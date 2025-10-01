namespace FairwayQuest.Core.Gameplay;

using FairwayQuest.Core.Models;

/// <summary>
/// Represents a configured round of golf with associated players and format.
/// </summary>
public sealed class Round
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Round"/> class.
    /// </summary>
    /// <param name="course">The course being played.</param>
    /// <param name="holes">The ordered set of holes.</param>
    /// <param name="players">The participating players.</param>
    /// <param name="format">The scoring format.</param>
    public Round(Course course, IReadOnlyList<Hole> holes, IReadOnlyList<Player> players, GameFormat format)
    {
        ArgumentNullException.ThrowIfNull(course);
        ArgumentNullException.ThrowIfNull(holes);
        ArgumentNullException.ThrowIfNull(players);

        if (players.Count == 0)
        {
            throw new ArgumentException("Rounds require at least one player.", nameof(players));
        }

        this.Course = course;
        this.Holes = holes;
        this.Players = players;
        this.Format = format;
    }

    /// <summary>Gets the course being played.</summary>
    public Course Course { get; }

    /// <summary>Gets the holes included in the round.</summary>
    public IReadOnlyList<Hole> Holes { get; }

    /// <summary>Gets the participating players.</summary>
    public IReadOnlyList<Player> Players { get; }

    /// <summary>Gets the scoring format for the round.</summary>
    public GameFormat Format { get; }
}
