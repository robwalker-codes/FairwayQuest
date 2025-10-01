namespace FairwayQuest.Cli.Round;

using System.Collections.Generic;
using System.Linq;
using FairwayQuest.Core.Gameplay;

/// <summary>
/// Tracks per-hole results for a player throughout the round.
/// </summary>
internal sealed class PlayerRoundTracker
{
    private readonly List<int> grossPerHole = new();
    private readonly List<int> netPerHole = new();
    private readonly List<int> stablefordPerHole = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerRoundTracker"/> class.
    /// </summary>
    /// <param name="player">The player being tracked.</param>
    public PlayerRoundTracker(Player player)
    {
        this.Player = player;
    }

    /// <summary>Gets the associated player.</summary>
    public Player Player { get; }

    /// <summary>Gets the recorded gross scores per hole.</summary>
    public IReadOnlyList<int> GrossPerHole => this.grossPerHole;

    /// <summary>Gets the recorded net scores per hole.</summary>
    public IReadOnlyList<int> NetPerHole => this.netPerHole;

    /// <summary>Gets the recorded Stableford points per hole.</summary>
    public IReadOnlyList<int> StablefordPerHole => this.stablefordPerHole;

    /// <summary>Gets the total gross strokes.</summary>
    public int TotalGrossStrokes => this.grossPerHole.Sum();

    /// <summary>Gets the total net strokes.</summary>
    public int TotalNetStrokes => this.netPerHole.Sum();

    /// <summary>Gets the total Stableford points.</summary>
    public int TotalStablefordPoints => this.stablefordPerHole.Sum();

    /// <summary>
    /// Records a stroke play result for the current hole.
    /// </summary>
    /// <param name="gross">The gross strokes taken.</param>
    /// <param name="net">The net strokes after allocations.</param>
    public void RecordStrokePlay(int gross, int net)
    {
        this.grossPerHole.Add(gross);
        this.netPerHole.Add(net);
    }

    /// <summary>
    /// Records a Stableford result for the current hole.
    /// </summary>
    /// <param name="gross">The gross strokes taken.</param>
    /// <param name="net">The net strokes after allocations.</param>
    /// <param name="points">The Stableford points earned.</param>
    public void RecordStableford(int gross, int net, int points)
    {
        this.RecordStrokePlay(gross, net);
        this.stablefordPerHole.Add(points);
    }
}
