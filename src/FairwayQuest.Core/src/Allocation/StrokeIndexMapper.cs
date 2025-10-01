namespace FairwayQuest.Core.Allocation;

using System.Linq;
using FairwayQuest.Core.Models;

/// <summary>
/// Builds stroke index rankings for a specific hole selection.
/// </summary>
public static class StrokeIndexMapper
{
    /// <summary>
    /// Computes the men's stroke index order for the provided hole sequence.
    /// </summary>
    /// <param name="holes">The holes being played.</param>
    /// <returns>The stroke index ranking mapped to each hole position.</returns>
    public static IReadOnlyList<int> MapMenStrokeIndexes(IReadOnlyList<Hole> holes)
    {
        ArgumentNullException.ThrowIfNull(holes);
        if (holes.Count == 0)
        {
            throw new ArgumentException("At least one hole is required.", nameof(holes));
        }

        var ordered = holes
            .Select((hole, index) => new { Hole = hole, Index = index })
            .OrderBy(tuple => tuple.Hole.StrokeIndex.Men)
            .ToList();

        var mapped = new int[holes.Count];
        for (var rank = 0; rank < ordered.Count; rank++)
        {
            mapped[ordered[rank].Index] = rank + 1;
        }

        return mapped;
    }
}
