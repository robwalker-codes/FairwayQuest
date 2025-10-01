namespace FairwayQuest.Tests.Support;

using System.Collections.Generic;
using FairwayQuest.Cli.Round;
using FairwayQuest.Core.Gameplay;
using FairwayQuest.Core.Models;

internal sealed class RecordingShotSelector : IShotSelector
{
    private readonly Queue<string> clubs;
    private readonly List<(string Player, double Remaining)> calls = new();

    public RecordingShotSelector(IEnumerable<string> clubs)
    {
        this.clubs = new Queue<string>(clubs);
    }

    public IReadOnlyList<(string Player, double Remaining)> Calls => calls;

    public string SelectClub(Player player, Hole hole, PlayerHoleState state)
    {
        var club = clubs.Dequeue();
        calls.Add((player.Name, state.RemainingYards));
        return club;
    }
}
