namespace FairwayQuest.Core.Shots;

/// <summary>
/// Provides metadata about a golf club.
/// </summary>
public sealed record ClubDefinition(string Code, string Label, double Min, double Max, ClubCategory Category, int DisplayOrder)
{
    /// <summary>Gets the average carry distance for the club.</summary>
    public double Average => (this.Min + this.Max) / 2d;

    /// <summary>Gets a value indicating whether the club is a putter.</summary>
    public bool IsPutter => this.Category == ClubCategory.Putter;

    /// <summary>Gets a value indicating whether the club is a wedge.</summary>
    public bool IsWedge => this.Category == ClubCategory.Wedge;

    /// <summary>Gets a value indicating whether the club is an iron.</summary>
    public bool IsIron => this.Category == ClubCategory.Iron;

    /// <summary>Gets a value indicating whether the club can realistically hole out from the fairway.</summary>
    public bool CanHoleOut => this.Category is ClubCategory.Iron or ClubCategory.Wedge;
}
