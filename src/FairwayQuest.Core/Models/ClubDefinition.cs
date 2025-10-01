namespace FairwayQuest.Core.Models;

public record ClubDefinition(string Code, int MinYards, int MaxYards)
{
    public int Midpoint => (MinYards + MaxYards) / 2;
}
