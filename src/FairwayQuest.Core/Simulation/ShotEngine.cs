using System.Globalization;
using FairwayQuest.Core.Models;

namespace FairwayQuest.Core.Simulation;

public class ShotEngine
{
    private const double MishitProbability = 0.07;
    private const double DuffProbability = 0.02;

    private const double RoughModifier = 0.9;
    private const double BunkerModifier = 0.85;
    private const double FairwayModifier = 1.0;
    private const double TeeModifier = 1.0;

    private const double CloseApproachThreshold = 8;
    private const double GreenApproachThreshold = 30;

    public ShotOutcome ExecuteShot(ShotState state, string clubInput, IRandomNumberGenerator rng)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(clubInput);
        ArgumentNullException.ThrowIfNull(rng);

        var clubCode = clubInput.Trim();

        if (state.IsOnGreen)
        {
            if (!string.Equals(clubCode, "p", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only the putter can be used on the green.");
            }

            return SimulatePutt(state, rng);
        }

        if (!ClubCatalog.TryGetClub(clubCode, out var club))
        {
            throw new ArgumentException($"Unknown club '{clubInput}'.", nameof(clubInput));
        }

        if (string.Equals(club.Code, "p", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Putter can only be used on the green.", nameof(clubInput));
        }

        var intended = SampleWithinRange(club.MinYards, club.MaxYards, rng);
        var lieModifier = state.Lie switch
        {
            Lie.Tee => TeeModifier,
            Lie.Fairway => FairwayModifier,
            Lie.Rough => RoughModifier,
            Lie.Bunker => BunkerModifier,
            Lie.Green => FairwayModifier,
            _ => 1.0
        };

        var mishitRoll = rng.NextDouble();
        double mishitFactor = 1.0;
        if (mishitRoll < DuffProbability)
        {
            mishitFactor = 0.05 + rng.NextDouble() * 0.15;
        }
        else if (mishitRoll < DuffProbability + MishitProbability)
        {
            mishitFactor = 0.35 + rng.NextDouble() * 0.25;
        }

        var carry = intended * lieModifier * mishitFactor;
        var remaining = state.RemainingYards - carry;
        var holed = remaining <= 0.5;
        if (holed)
        {
            var description = $"{club.Code.ToLower(CultureInfo.InvariantCulture)} → holed";
            return new ShotOutcome(new ShotState(0, Lie.Green), 1, description, true);
        }

        var adjustedRemaining = Math.Max(0.5, remaining);
        var newLie = DetermineNextLie(state.Lie, adjustedRemaining, rng);
        if (adjustedRemaining <= CloseApproachThreshold)
        {
            newLie = Lie.Green;
        }

        var descriptionLine = string.Format(CultureInfo.InvariantCulture,
            "{0} → {1}y, {2}, {3}y to pin",
            club.Code.ToLower(CultureInfo.InvariantCulture),
            Math.Round(carry),
            newLie.ToString().ToLower(CultureInfo.InvariantCulture),
            Math.Round(adjustedRemaining));

        return new ShotOutcome(new ShotState(adjustedRemaining, newLie), 1, descriptionLine, false);
    }

    private static ShotOutcome SimulatePutt(ShotState state, IRandomNumberGenerator rng)
    {
        var start = state.RemainingYards;
        int putts;
        if (start <= 2)
        {
            putts = rng.NextDouble() < 0.8 ? 1 : 2;
        }
        else if (start <= 8)
        {
            putts = rng.NextDouble() < 0.45 ? 1 : 2;
        }
        else
        {
            putts = rng.NextDouble() < 0.7 ? 2 : 3;
        }

        var description = $"p → holed ({putts} {(putts == 1 ? "putt" : "putts")})";
        return new ShotOutcome(new ShotState(0, Lie.Green), putts, description, true);
    }

    private static double SampleWithinRange(double min, double max, IRandomNumberGenerator rng)
    {
        return min + (max - min) * rng.NextDouble();
    }

    private static Lie DetermineNextLie(Lie previousLie, double remaining, IRandomNumberGenerator rng)
    {
        if (remaining <= GreenApproachThreshold)
        {
            var roll = rng.NextDouble();
            if (remaining <= CloseApproachThreshold || roll < 0.5)
            {
                return Lie.Green;
            }

            return roll < 0.8 ? Lie.Fairway : Lie.Rough;
        }

        var lieRoll = rng.NextDouble();
        if (lieRoll < 0.7)
        {
            return Lie.Fairway;
        }

        if (lieRoll < 0.9)
        {
            return Lie.Rough;
        }

        return Lie.Bunker;
    }
}
