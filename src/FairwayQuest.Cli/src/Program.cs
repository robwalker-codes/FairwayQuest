namespace FairwayQuest.Cli;

using System.Diagnostics.CodeAnalysis;
using FairwayQuest.Cli.Random;
using FairwayQuest.Core.Abstractions;
using FairwayQuest.Courses;

#pragma warning disable CA1303 // CLI outputs literal strings by design.

/// <summary>
/// Entry point for the FairwayQuest command-line application.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Zero for success; non-zero otherwise.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "CLI entry point surfaces user-friendly messages.")]
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var options = ArgumentParser.Parse(args);
            var repository = new JsonCourseRepository(ResolveCoursesPath());
            var randomProvider = new SeedRandomProvider(options.Seed);
            var application = new Application(repository, randomProvider, options);
            await application.RunAsync().ConfigureAwait(false);
            return 0;
        }
        catch (OperationCanceledException)
        {
            await Console.Out.WriteLineAsync(string.Empty).ConfigureAwait(false);
            await Console.Out.WriteLineAsync("Round aborted.").ConfigureAwait(false);
            return 1;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}").ConfigureAwait(false);
            return 1;
        }
    }

    /// <summary>
    /// Determines the directory containing course JSON files.
    /// </summary>
    /// <returns>The resolved path.</returns>
    private static string ResolveCoursesPath()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.CurrentDirectory, "courses"),
            Path.Combine(AppContext.BaseDirectory, "courses"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "courses")),
        };

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return candidates[0];
    }
}

#pragma warning restore CA1303
