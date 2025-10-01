namespace FairwayQuest.Core.Models;

/// <summary>
/// Identifies the supported scoring formats.
/// </summary>
public enum GameFormat
{
    /// <summary>
    /// Traditional stroke play where gross strokes are aggregated.
    /// </summary>
    StrokePlay,

    /// <summary>
    /// Stableford scoring where holes award points based on net score relative to par.
    /// </summary>
    Stableford,
}
