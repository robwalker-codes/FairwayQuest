namespace FairwayQuest.Core.Gameplay;

/// <summary>
/// Represents a player participating in a round.
/// </summary>
public sealed class Player
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    /// <param name="name">The player name.</param>
    /// <param name="handicapIndex18">The 18-hole handicap index.</param>
    /// <param name="tee">The selected tee identifier.</param>
    /// <param name="effectivePlayingHandicap">The effective playing handicap for the round.</param>
    /// <param name="allocatedStrokesPerHole">The allocated strokes per hole.</param>
    public Player(
        string name,
        int handicapIndex18,
        string tee,
        int effectivePlayingHandicap,
        IReadOnlyList<int> allocatedStrokesPerHole)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(tee);
        ArgumentNullException.ThrowIfNull(allocatedStrokesPerHole);

        this.Name = name;
        this.HandicapIndex18 = handicapIndex18;
        this.Tee = tee;
        this.EffectivePlayingHandicap = effectivePlayingHandicap;
        this.AllocatedStrokesPerHole = allocatedStrokesPerHole;
    }

    /// <summary>Gets the player name.</summary>
    public string Name { get; }

    /// <summary>Gets the eighteen-hole handicap index.</summary>
    public int HandicapIndex18 { get; }

    /// <summary>Gets the selected tee identifier.</summary>
    public string Tee { get; }

    /// <summary>Gets the effective playing handicap for the round.</summary>
    public int EffectivePlayingHandicap { get; }

    /// <summary>Gets the per-hole stroke allocations.</summary>
    public IReadOnlyList<int> AllocatedStrokesPerHole { get; }
}
