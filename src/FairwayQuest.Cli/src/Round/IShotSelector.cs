namespace FairwayQuest.Cli.Round;

using FairwayQuest.Core.Gameplay;
using FairwayQuest.Core.Models;

/// <summary>
/// Abstraction for retrieving the club selection used during interactive play.
/// </summary>
public interface IShotSelector
{
    /// <summary>
    /// Selects the club code to be used for the next shot.
    /// </summary>
    /// <param name="player">The player taking the shot.</param>
    /// <param name="hole">The hole currently in play.</param>
    /// <param name="state">The player's per-hole state.</param>
    /// <returns>The club code to feed into the shot engine.</returns>
    string SelectClub(Player player, Hole hole, PlayerHoleState state);
}
