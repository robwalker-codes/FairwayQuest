using System;
using System.Collections.Generic;
using System.Linq;

namespace FairwayQuest.Core.Scoring;

public static class Handicap
{
    public static int ComputeEffectiveHandicap(
        int handicapIndex18,
        int holes,
        bool isStableford,
        int stablefordAllowancePercent = 95)
    {
        if (holes != 9 && holes != 18)
        {
            throw new ArgumentOutOfRangeException(nameof(holes), "Only 9 or 18 hole rounds are supported.");
        }

        var playingHandicap = holes == 18
            ? handicapIndex18
            : (int)Math.Round(handicapIndex18 / 2.0, MidpointRounding.ToEven);

        var effective = playingHandicap;
        if (isStableford)
        {
            var allowanceMultiplier = stablefordAllowancePercent / 100.0;
            effective = RoundHalfAwayFromZero(playingHandicap * allowanceMultiplier);
        }

        return Math.Clamp(effective, 0, 54);
    }

    public static int[] AllocateStrokesPerHole(int effectiveHandicap, IReadOnlyList<int> strokeIndexes)
    {
        ArgumentNullException.ThrowIfNull(strokeIndexes);

        var holes = strokeIndexes.Count;
        if (holes == 0)
        {
            return Array.Empty<int>();
        }

        if (effectiveHandicap <= 0)
        {
            return new int[holes];
        }

        var strokes = new int[holes];
        var baseStrokes = Math.DivRem(effectiveHandicap, holes, out var extras);

        for (var i = 0; i < holes; i++)
        {
            strokes[i] = baseStrokes;
        }

        if (extras == 0)
        {
            return strokes;
        }

        var ordered = strokeIndexes
            .Select((strokeIndex, index) => (strokeIndex, index))
            .OrderBy(tuple => tuple.strokeIndex)
            .ThenBy(tuple => tuple.index)
            .ToArray();

        for (var i = 0; i < extras; i++)
        {
            var index = ordered[i].index;
            strokes[index] += 1;
        }

        return strokes;
    }

    internal static int RoundHalfAwayFromZero(double value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);
}
