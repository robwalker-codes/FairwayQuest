using System.Linq;
using FairwayQuest.Core.Models;

namespace FairwayQuest.Core.Scoring;

public static class HandicapAllocator
{
    public static int[] Allocate(int handicap, IReadOnlyList<Hole> holes)
    {
        ArgumentNullException.ThrowIfNull(holes);
        if (handicap < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(handicap));
        }

        var count = holes.Count;
        if (count == 0)
        {
            return Array.Empty<int>();
        }

        var strokes = new int[count];
        if (handicap == 0)
        {
            return strokes;
        }

        var fullCycles = handicap / count;
        var remainder = handicap % count;

        for (var i = 0; i < count; i++)
        {
            strokes[i] = fullCycles;
        }

        if (remainder == 0)
        {
            return strokes;
        }

        var ordered = holes
            .Select((hole, index) => (hole, index))
            .OrderBy(tuple => tuple.hole.StrokeIndex)
            .ToArray();

        for (var i = 0; i < remainder; i++)
        {
            var (_, index) = ordered[i % ordered.Length];
            strokes[index]++;
        }

        return strokes;
    }
}
