namespace FairwayQuest.Core.Models;

/// <summary>
/// Represents the stroke index ranking for a hole.
/// </summary>
public sealed class StrokeIndexSet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StrokeIndexSet"/> class.
    /// </summary>
    /// <param name="men">The stroke index used for men's play.</param>
    /// <param name="women">The stroke index used for women's play.</param>
    public StrokeIndexSet(int men, int? women)
    {
        if (men < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(men), men, "Stroke index must be at least 1.");
        }

        if (women is < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(women), women, "Stroke index must be at least 1 when provided.");
        }

        Men = men;
        Women = women;
    }

    /// <summary>
    /// Gets the stroke index used for men's play.
    /// </summary>
    public int Men { get; }

    /// <summary>
    /// Gets the stroke index used for women's play, when available.
    /// </summary>
    public int? Women { get; }
}
