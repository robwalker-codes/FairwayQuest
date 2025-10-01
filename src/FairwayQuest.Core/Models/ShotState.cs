namespace FairwayQuest.Core.Models;

public record ShotState(double RemainingYards, Lie Lie)
{
    public bool IsOnGreen => Lie == Lie.Green;

    public static ShotState ForHoleStart(int yardage) => new(yardage, Lie.Tee);
}
