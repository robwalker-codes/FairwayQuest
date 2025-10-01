namespace FairwayQuest.Core.Shots;

/// <summary>
/// Represents the current lie of the golf ball.
/// </summary>
public enum Lie
{
    /// <summary>Ball is on the tee box.</summary>
    Tee,

    /// <summary>Ball rests on the fairway.</summary>
    Fairway,

    /// <summary>Ball lies in light rough.</summary>
    Rough,

    /// <summary>Ball lies in heavy rough.</summary>
    DeepRough,

    /// <summary>Ball is in a bunker.</summary>
    Bunker,

    /// <summary>Ball rests on the fringe.</summary>
    Fringe,

    /// <summary>Ball is on the putting green.</summary>
    Green,
}
