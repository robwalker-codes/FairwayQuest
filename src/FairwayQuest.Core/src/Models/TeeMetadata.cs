namespace FairwayQuest.Core.Models;

/// <summary>
/// Describes course rating information for a tee set.
/// </summary>
public sealed class TeeMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TeeMetadata"/> class.
    /// </summary>
    /// <param name="ratingMen">The course rating for men.</param>
    /// <param name="slopeMen">The slope rating for men.</param>
    public TeeMetadata(double? ratingMen, int? slopeMen)
    {
        RatingMen = ratingMen;
        SlopeMen = slopeMen;
    }

    /// <summary>
    /// Gets the course rating for men.
    /// </summary>
    public double? RatingMen { get; }

    /// <summary>
    /// Gets the slope rating for men.
    /// </summary>
    public int? SlopeMen { get; }
}
