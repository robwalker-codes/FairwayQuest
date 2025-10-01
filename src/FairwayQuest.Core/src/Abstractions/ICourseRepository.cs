namespace FairwayQuest.Core.Abstractions;

using FairwayQuest.Core.Models;

/// <summary>
/// Provides access to <see cref="Course"/> definitions available to the application.
/// </summary>
public interface ICourseRepository
{
    /// <summary>
    /// Loads all configured courses.
    /// </summary>
    /// <param name="cancellationToken">Token used to observe cancellation.</param>
    /// <returns>A read-only collection of courses.</returns>
    Task<IReadOnlyList<Course>> GetCoursesAsync(CancellationToken cancellationToken = default);
}
