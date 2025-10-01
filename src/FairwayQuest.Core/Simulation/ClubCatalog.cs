using System.Collections.Generic;
using System.Linq;
using FairwayQuest.Core.Models;

namespace FairwayQuest.Core.Simulation;

public static class ClubCatalog
{
    private static readonly Dictionary<string, ClubDefinition> Clubs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["D"] = new("D", 230, 280),
        ["3w"] = new("3w", 210, 240),
        ["5w"] = new("5w", 195, 215),
        ["3i"] = new("3i", 180, 200),
        ["4i"] = new("4i", 170, 190),
        ["5i"] = new("5i", 160, 180),
        ["6i"] = new("6i", 150, 170),
        ["7i"] = new("7i", 140, 160),
        ["8i"] = new("8i", 130, 150),
        ["9i"] = new("9i", 120, 140),
        ["pw"] = new("pw", 95, 115),
        ["sw"] = new("sw", 70, 95),
        ["lw"] = new("lw", 40, 70),
        ["p"] = new("p", 1, 1)
    };

    public static IReadOnlyCollection<ClubDefinition> AllClubs => Clubs.Values;

    public static bool TryGetClub(string input, out ClubDefinition definition)
    {
        return Clubs.TryGetValue(input.Trim(), out definition!);
    }

    public static IEnumerable<string> NonPutterCodes => Clubs.Keys.Where(k => !string.Equals(k, "p", StringComparison.OrdinalIgnoreCase)).OrderBy(k => k);
}
