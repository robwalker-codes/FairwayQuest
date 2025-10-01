namespace FairwayQuest.Core.Scoring;

public static class ScoreCalculator
{
    public static int CalculateNetStrokes(int grossStrokes, int strokesReceived)
    {
        var net = grossStrokes - strokesReceived;
        return net < 1 ? 1 : net;
    }

    public static int StablefordPointsFromRelativeScore(int relativeToPar)
    {
        return relativeToPar switch
        {
            <= -4 => 6,
            -3 => 5,
            -2 => 4,
            -1 => 3,
            0 => 2,
            1 => 1,
            >= 2 => 0
        };
    }
}
