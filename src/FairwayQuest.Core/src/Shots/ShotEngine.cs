namespace FairwayQuest.Core.Shots;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FairwayQuest.Core.Abstractions;

/// <summary>
/// Simulates golf shots using simplified physics and probabilistic outcomes.
/// </summary>
public sealed class ShotEngine
{
    private const double HoleOutProximity = 3d;
    private const double HoleOutProbability = 0.01d;
    private const double MishitProbability = 0.07d;
    private const double DuffProbability = 0.02d;
    private const double PenaltyProbability = 0.015d;

    private static readonly Dictionary<string, ClubDefinition> Clubs = BuildClubMap();

    private static readonly Dictionary<Lie, double> LieDistanceModifiers = new Dictionary<Lie, double>
    {
        [Lie.Tee] = 0d,
        [Lie.Fairway] = 0d,
        [Lie.Rough] = -0.10d,
        [Lie.DeepRough] = -0.20d,
        [Lie.Bunker] = -0.15d,
        [Lie.Fringe] = -0.05d,
        [Lie.Green] = 0d,
    };

    /// <summary>
    /// Returns the metadata describing the supported clubs.
    /// </summary>
    /// <returns>An ordered list of club definitions.</returns>
    public static IReadOnlyList<ClubDefinition> GetSupportedClubs() => Clubs.Values.OrderBy(c => c.DisplayOrder).ToList();

    /// <summary>
    /// Executes a shot simulation for the provided context.
    /// </summary>
    /// <param name="request">The shot request describing the current lie and club.</param>
    /// <param name="random">The random provider supplying deterministic values.</param>
    /// <returns>The resulting shot outcome.</returns>
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance usage simplifies DI patterns.")]
    public ShotResult Execute(ShotRequest request, IRandomProvider random)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(random);

        var club = ResolveClub(request.Club);
        return club.IsPutter
            ? ResolvePuttingShot(request, random, club)
            : ResolveFullSwingShot(request, random, club);
    }

    private static ClubDefinition ResolveClub(string code)
    {
        var key = code.Trim();
        if (!Clubs.TryGetValue(key, out var definition))
        {
            throw new ArgumentException($"Unsupported club '{code}'.", nameof(code));
        }

        return definition;
    }

    private ShotResult ResolveFullSwingShot(ShotRequest request, IRandomProvider random, ClubDefinition club)
    {
        if (IsPenalty(random))
        {
            return CreatePenaltyResult(request, club);
        }

        var distance = CalculateTravelDistance(request, random, club, out var contactLabel);
        var remaining = CalculateRemaining(request.RemainingYards, distance);
        var lateral = ResolveLateral(random);
        var holed = DetermineHoleOut(random, club, distance, request.RemainingYards);
        var newLie = holed ? Lie.Green : DetermineLandingLie(club, remaining, random);
        var commentary = BuildCommentary(club, distance, lateral, newLie, remaining, contactLabel, holed, false);

        return new ShotResult(1, 0, distance, lateral, newLie, remaining, holed, commentary);
    }

    private static bool IsPenalty(IRandomProvider random) => random.NextDouble() < PenaltyProbability;

    private static ShotResult CreatePenaltyResult(ShotRequest request, ClubDefinition club)
    {
        var commentary = $"{club.Label} → OB (penalty +1); {request.RemainingYards:0}y remaining";
        return new ShotResult(1, 1, 0, "penalty", Lie.Rough, request.RemainingYards, false, commentary);
    }

    private static ShotResult ResolvePuttingShot(ShotRequest request, IRandomProvider random, ClubDefinition club)
    {
        var putts = ResolvePuttCount(request.RemainingYards, random);
        var commentary = putts switch
        {
            1 => "p → holed",  // short, direct message
            _ => $"p → holed ({putts} putts)",
        };

        return new ShotResult(putts, 0, request.RemainingYards, "straight", Lie.Green, 0, true, commentary);
    }

    private static int ResolvePuttCount(double remaining, IRandomProvider random)
    {
        var roll = random.NextDouble();
        if (remaining <= 2)
        {
            return roll < 0.8 ? 1 : roll < 0.95 ? 2 : 3;
        }

        if (remaining <= 8)
        {
            return roll < 0.55 ? 1 : 2;
        }

        return roll < 0.7 ? 2 : 3;
    }

    private double CalculateTravelDistance(ShotRequest request, IRandomProvider random, ClubDefinition club, out string contactLabel)
    {
        var baseDistance = SampleClubDistance(club, random);
        var lieAdjusted = ApplyLieModifier(baseDistance, request.Lie);
        var contactAdjusted = ApplyContactVariance(lieAdjusted, random, out contactLabel);
        var clamped = Math.Max(0, contactAdjusted);
        return LimitOvershoot(clamped, request.RemainingYards);
    }

    private static double SampleClubDistance(ClubDefinition club, IRandomProvider random)
    {
        var span = club.Max - club.Min;
        var distance = club.Min + (span * random.NextDouble());
        return (distance + (club.Average * 0.4)) / 1.4;
    }

    private static double ApplyLieModifier(double distance, Lie lie)
    {
        return distance * (1 + LieDistanceModifiers[lie]);
    }

    private static double ApplyContactVariance(double distance, IRandomProvider random, out string contactLabel)
    {
        var roll = random.NextDouble();
        if (roll < DuffProbability)
        {
            return ResolveDuff(distance, random, out contactLabel);
        }

        if (roll < DuffProbability + MishitProbability)
        {
            return ResolveMishit(distance, random, out contactLabel);
        }

        return ResolvePureContact(distance, random, out contactLabel);
    }

    private static double ResolveDuff(double distance, IRandomProvider random, out string contactLabel)
    {
        contactLabel = "duffed";
        return distance * random.NextDouble(0.05, 0.2);
    }

    private static double ResolveMishit(double distance, IRandomProvider random, out string contactLabel)
    {
        contactLabel = "mishit";
        return distance * random.NextDouble(0.35, 0.6);
    }

    private static double ResolvePureContact(double distance, IRandomProvider random, out string contactLabel)
    {
        contactLabel = "pure";
        return distance * random.NextDouble(0.9, 1.05);
    }

    private static double LimitOvershoot(double distance, double remaining)
    {
        return Math.Min(distance, remaining + 30);
    }

    private static double CalculateRemaining(double starting, double travelled)
    {
        return Math.Max(0, starting - travelled);
    }

    private static string ResolveLateral(IRandomProvider random)
    {
        var roll = random.NextDouble();
        return roll switch
        {
            < 0.65 => "straight",
            < 0.8 => random.NextDouble() < 0.5 ? "pull" : "push",
            < 0.92 => random.NextDouble() < 0.5 ? "draw" : "fade",
            < 0.97 => random.NextDouble() < 0.5 ? "hook" : "slice",
            _ => "straight",
        };
    }

    private static bool DetermineHoleOut(IRandomProvider random, ClubDefinition club, double distance, double remaining)
    {
        if (remaining > HoleOutProximity + 5)
        {
            return false;
        }

        if (!club.CanHoleOut)
        {
            return false;
        }

        if (Math.Abs(distance - remaining) > HoleOutProximity)
        {
            return false;
        }

        return random.NextDouble() < HoleOutProbability;
    }

    private static Lie DetermineLandingLie(ClubDefinition club, double remaining, IRandomProvider random)
    {
        if (remaining <= 2)
        {
            return Lie.Green;
        }

        if (remaining <= 8)
        {
            return ResolveNearGreenLanding(random);
        }

        if (remaining <= 30)
        {
            return ResolveApproachLanding(random, club);
        }

        return ResolveLongLanding(random, club);
    }

    private static Lie ResolveNearGreenLanding(IRandomProvider random)
    {
        return random.NextDouble() < 0.7 ? Lie.Green : Lie.Fringe;
    }

    private static Lie ResolveApproachLanding(IRandomProvider random, ClubDefinition club)
    {
        var wedgeBias = club.IsWedge ? 0.5 : 0.3;
        var roll = random.NextDouble();
        if (roll < wedgeBias)
        {
            return Lie.Fringe;
        }

        if (roll < wedgeBias + 0.3)
        {
            return Lie.Green;
        }

        return random.NextDouble() < 0.5 ? Lie.Fairway : Lie.Rough;
    }

    private static Lie ResolveLongLanding(IRandomProvider random, ClubDefinition club)
    {
        var roll = random.NextDouble();
        if (roll < 0.6)
        {
            return Lie.Fairway;
        }

        if (roll < 0.82)
        {
            return Lie.Rough;
        }

        if (roll < 0.92)
        {
            return Lie.DeepRough;
        }

        return club.IsWedge || club.IsIron ? Lie.Fringe : Lie.Bunker;
    }

    private static string BuildCommentary(
        ClubDefinition club,
        double distance,
        string lateral,
        Lie newLie,
        double remaining,
        string contact,
        bool holed,
        bool penalty)
    {
        if (penalty)
        {
            return BuildPenaltyCommentary(club, remaining);
        }

        if (holed)
        {
            return BuildHoledCommentary(club);
        }

        return BuildStandardCommentary(club, distance, lateral, newLie, remaining, contact);
    }

    private static string BuildPenaltyCommentary(ClubDefinition club, double remaining)
    {
        return $"{club.Label} → penalty; {remaining:0}y remaining";
    }

    private static string BuildHoledCommentary(ClubDefinition club)
    {
        return club.IsPutter ? "p → holed" : $"{club.Label} → holed!";
    }

    private static string BuildStandardCommentary(ClubDefinition club, double distance, string lateral, Lie newLie, double remaining, string contact)
    {
        var contactText = contact == "pure" ? string.Empty : $"{contact} ";
        var message = $"{club.Label} → {contactText}{distance:0}y, {lateral}, {newLie}, {remaining:0}y to pin";
        return message.Replace("  ", " ", StringComparison.Ordinal);
    }

    private static Dictionary<string, ClubDefinition> BuildClubMap()
    {
        var clubs = new[]
        {
            new ClubDefinition("d", "D", 230, 280, ClubCategory.Wood, 1),
            new ClubDefinition("3w", "3W", 210, 240, ClubCategory.Wood, 2),
            new ClubDefinition("5w", "5W", 195, 215, ClubCategory.Wood, 3),
            new ClubDefinition("3i", "3i", 180, 200, ClubCategory.Iron, 4),
            new ClubDefinition("4i", "4i", 170, 190, ClubCategory.Iron, 5),
            new ClubDefinition("5i", "5i", 160, 180, ClubCategory.Iron, 6),
            new ClubDefinition("6i", "6i", 150, 170, ClubCategory.Iron, 7),
            new ClubDefinition("7i", "7i", 140, 160, ClubCategory.Iron, 8),
            new ClubDefinition("8i", "8i", 130, 150, ClubCategory.Iron, 9),
            new ClubDefinition("9i", "9i", 120, 140, ClubCategory.Iron, 10),
            new ClubDefinition("pw", "PW", 95, 115, ClubCategory.Wedge, 11),
            new ClubDefinition("sw", "SW", 70, 95, ClubCategory.Wedge, 12),
            new ClubDefinition("lw", "LW", 40, 70, ClubCategory.Wedge, 13),
            new ClubDefinition("p", "p", 0, 0, ClubCategory.Putter, 14),
        };

        return clubs.ToDictionary(club => club.Code, StringComparer.OrdinalIgnoreCase);
    }
}
