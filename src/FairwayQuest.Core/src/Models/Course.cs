namespace FairwayQuest.Core.Models;

using System.Collections.ObjectModel;
using System.Linq;

/// <summary>
/// Represents a golf course comprised of up to eighteen holes.
/// </summary>
public sealed class Course
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Course"/> class.
    /// </summary>
    /// <param name="name">The course name.</param>
    /// <param name="location">The course location.</param>
    /// <param name="holes">The ordered collection of holes.</param>
    /// <param name="defaultTee">The default tee identifier.</param>
    /// <param name="teeMetadata">Metadata describing available tee sets.</param>
    public Course(
        string name,
        string location,
        IReadOnlyList<Hole> holes,
        string defaultTee,
        IReadOnlyDictionary<string, TeeMetadata> teeMetadata)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultTee);
        ArgumentNullException.ThrowIfNull(holes);
        ArgumentNullException.ThrowIfNull(teeMetadata);

        if (holes.Count is not 9 and not 18)
        {
            throw new ArgumentException("Courses must contain 9 or 18 holes.", nameof(holes));
        }

        if (!teeMetadata.ContainsKey(defaultTee))
        {
            throw new ArgumentException("Default tee must be present in tee metadata.", nameof(defaultTee));
        }

        this.Name = name;
        this.Location = location;
        this.Holes = new ReadOnlyCollection<Hole>(holes.ToList());
        this.DefaultTee = defaultTee;
        this.TeesMetadata = new ReadOnlyDictionary<string, TeeMetadata>(new Dictionary<string, TeeMetadata>(teeMetadata, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the course name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the course location.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// Gets the default tee identifier.
    /// </summary>
    public string DefaultTee { get; }

    /// <summary>
    /// Gets metadata describing the tee sets available for this course.
    /// </summary>
    public IReadOnlyDictionary<string, TeeMetadata> TeesMetadata { get; }

    /// <summary>
    /// Gets the ordered hole collection.
    /// </summary>
    public IReadOnlyList<Hole> Holes { get; }

    /// <summary>
    /// Returns the ordered set of holes for the specified selection.
    /// </summary>
    /// <param name="selection">The hole selection.</param>
    /// <returns>The matching hole sequence.</returns>
    public IReadOnlyList<Hole> SelectHoles(HoleSelection selection)
    {
        return selection switch
        {
            HoleSelection.All18 => this.Holes,
            HoleSelection.Front9 => this.Holes.Take(9).ToList(),
            HoleSelection.Back9 => this.Holes.Skip(Math.Max(0, this.Holes.Count - 9)).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, "Unsupported selection."),
        };
    }
}
