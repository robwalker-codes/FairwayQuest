namespace FairwayQuest.Cli;

/// <summary>
/// Captures command-line options influencing gameplay.
/// </summary>
internal sealed class AppOptions
{
    /// <summary>Gets the deterministic seed value.</summary>
    public int? Seed { get; init; }

    /// <summary>Gets a value indicating whether narration should be condensed.</summary>
    public bool FastMode { get; init; }

    /// <summary>Gets a value indicating whether auto-play is enabled.</summary>
    public bool AutoPlay { get; init; }
}
