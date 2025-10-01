using FairwayQuest.Core.Models;
using FairwayQuest.Core.Simulation;

namespace FairwayQuest.Core.Validation;

public static class InputParsers
{
    public static bool TryParsePlayerCount(string? input, out int count)
    {
        count = 0;
        if (!int.TryParse(input, out var parsed))
        {
            return false;
        }

        if (parsed is < 1 or > 4)
        {
            return false;
        }

        count = parsed;
        return true;
    }

    public static bool TryParseHoleCount(string? input, out int holes)
    {
        holes = 0;
        if (!int.TryParse(input, out var parsed))
        {
            return false;
        }

        if (parsed is not (9 or 18))
        {
            return false;
        }

        holes = parsed;
        return true;
    }

    public static bool TryParseHandicap(string? input, out int handicap)
    {
        handicap = 0;
        if (!int.TryParse(input, out var parsed))
        {
            return false;
        }

        if (parsed is < 0 or > 54)
        {
            return false;
        }

        handicap = parsed;
        return true;
    }

    public static bool TryParseGameType(string? input, out GameType gameType)
    {
        gameType = GameType.Stroke;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = input.Trim().ToLowerInvariant();
        if (normalized == "stroke")
        {
            gameType = GameType.Stroke;
            return true;
        }

        if (normalized == "stableford")
        {
            gameType = GameType.Stableford;
            return true;
        }

        return false;
    }

    public static bool TryParseClub(string? input, bool onGreen, out string normalizedClub)
    {
        normalizedClub = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var trimmed = input.Trim();
        if (trimmed == "?")
        {
            return false;
        }

        if (onGreen)
        {
            if (string.Equals(trimmed, "p", StringComparison.OrdinalIgnoreCase))
            {
                normalizedClub = "p";
                return true;
            }

            return false;
        }

        if (!ClubCatalog.TryGetClub(trimmed, out var club))
        {
            return false;
        }

        if (string.Equals(club.Code, "p", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        normalizedClub = club.Code;
        return true;
    }
}
