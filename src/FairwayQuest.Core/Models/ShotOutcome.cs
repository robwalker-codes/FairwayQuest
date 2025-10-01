namespace FairwayQuest.Core.Models;

public record ShotOutcome(ShotState NewState, int StrokesUsed, string Description, bool Holed);
