namespace FairwayQuest.Cli;

/// <summary>
/// Parses command-line arguments into <see cref="AppOptions"/>.
/// </summary>
internal static class ArgumentParser
{
    /// <summary>
    /// Parses the provided argument array.
    /// </summary>
    /// <param name="args">Raw command-line arguments.</param>
    /// <returns>Populated <see cref="AppOptions"/>.</returns>
    public static AppOptions Parse(string[] args)
    {
        var options = new AppOptions();
        if (args.Length == 0)
        {
            return options;
        }

        var seedValue = default(int?);
        var fast = false;
        var auto = false;

        for (var i = 0; i < args.Length; i++)
        {
            var argument = args[i];
            switch (argument)
            {
                case "--seed":
                    if (i + 1 >= args.Length)
                    {
                        throw new ArgumentException("--seed requires a numeric value.");
                    }

                    if (!int.TryParse(args[++i], out var parsedSeed))
                    {
                        throw new ArgumentException("--seed value must be an integer.");
                    }

                    seedValue = parsedSeed;
                    break;
                case "--fast":
                    fast = true;
                    break;
                case "--auto":
                    auto = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{argument}'.");
            }
        }

        return new AppOptions
        {
            Seed = seedValue,
            FastMode = fast,
            AutoPlay = auto,
        };
    }
}
