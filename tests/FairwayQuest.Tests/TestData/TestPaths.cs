namespace FairwayQuest.Tests.TestData;

using System;
using System.IO;

public static class TestPaths
{
    public static string CoursesDirectory => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "courses"));
}
