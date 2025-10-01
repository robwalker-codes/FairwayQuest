namespace FairwayQuest.Core.Gameplay;

using System.Linq;
using FairwayQuest.Core.Models;
using FairwayQuest.Core.Shots;

/// <summary>
/// Represents the live play state for a single hole across all players.
/// </summary>
public sealed class HolePlayState
{
    private readonly IReadOnlyList<PlayerHoleState> playerStates;

    /// <summary>
    /// Initializes a new instance of the <see cref="HolePlayState"/> class.
    /// </summary>
    /// <param name="hole">The hole being played.</param>
    /// <param name="players">The players in turn order.</param>
    /// <param name="playerStates">The per-player state collection.</param>
    public HolePlayState(Hole hole, IReadOnlyList<Player> players, IReadOnlyList<PlayerHoleState> playerStates)
    {
        ArgumentNullException.ThrowIfNull(hole);
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(playerStates);

        if (players.Count != playerStates.Count)
        {
            throw new ArgumentException("Players and states must align in length.");
        }

        this.Hole = hole;
        this.playerStates = playerStates;
    }

    /// <summary>Gets the hole being played.</summary>
    public Hole Hole { get; }

    /// <summary>Gets the per-player state collection.</summary>
    public IReadOnlyList<PlayerHoleState> PlayerStates => this.playerStates;

    /// <summary>Gets a value indicating whether all players have holed out.</summary>
    public bool IsComplete => this.playerStates.All(state => state.IsHoled);

    /// <summary>
    /// Retrieves the state for a specific player index.
    /// </summary>
    /// <param name="playerIndex">The zero-based player index.</param>
    /// <returns>The corresponding player state.</returns>
    public PlayerHoleState GetStateForPlayer(int playerIndex)
    {
        return this.playerStates[playerIndex];
    }
}
