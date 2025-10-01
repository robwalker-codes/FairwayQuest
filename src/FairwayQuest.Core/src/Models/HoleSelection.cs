namespace FairwayQuest.Core.Models;

/// <summary>
/// Identifies which part of an eighteen-hole course will be played.
/// </summary>
public enum HoleSelection
{
    /// <summary>
    /// All eighteen holes.
    /// </summary>
    All18,

    /// <summary>
    /// The front nine (holes 1-9).
    /// </summary>
    Front9,

    /// <summary>
    /// The back nine (holes 10-18).
    /// </summary>
    Back9,
}
