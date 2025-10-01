namespace FairwayQuest.Core.Models;

using System.Collections.ObjectModel;

/// <summary>
/// Represents a single golf hole.
/// </summary>
public sealed class Hole
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Hole"/> class.
    /// </summary>
    /// <param name="number">The hole number.</param>
    /// <param name="par">The par for the hole.</param>
    /// <param name="strokeIndex">The stroke index information for the hole.</param>
    /// <param name="yards">The tee yardages keyed by tee name.</param>
    public Hole(int number, int par, StrokeIndexSet strokeIndex, IReadOnlyDictionary<string, int> yards)
    {
        if (number < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(number), number, "Hole numbers must start at 1.");
        }

        if (par < 3)
        {
            throw new ArgumentOutOfRangeException(nameof(par), par, "Par must be at least 3.");
        }

        ArgumentNullException.ThrowIfNull(strokeIndex);
        ArgumentNullException.ThrowIfNull(yards);

        this.Number = number;
        this.Par = par;
        this.StrokeIndex = strokeIndex;
        this.Yards = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(yards, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the hole number.
    /// </summary>
    public int Number { get; }

    /// <summary>
    /// Gets the hole par.
    /// </summary>
    public int Par { get; }

    /// <summary>
    /// Gets the stroke index information.
    /// </summary>
    public StrokeIndexSet StrokeIndex { get; }

    /// <summary>
    /// Gets the tee yardages keyed by tee name.
    /// </summary>
    public IReadOnlyDictionary<string, int> Yards { get; }
}
